using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows.Media;
using CommonBehaviors.Actions;
using Mandalore.Helpers;
using Mandalore.Movement;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.CommonBot.POI;
using Styx.CommonBot.Routines;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace Mandalore
{
    public abstract class Mandalore : CombatRoutine
    {
        public static readonly LocalPlayer Me = StyxWoW.Me;
        #region Required Implementations

        public override string Name => "Mandalore";
        public override bool WantButton { get { return true; } }
        public override WoWClass Class => WoWClass.None;

        public override CapabilityFlags SupportedCapabilities => CapabilityFlags.All;
        protected static bool IsAllowed(CapabilityFlags flags) => RoutineManager.GetCapabilityState(flags) == CapabilityState.DontCare;
        
        #endregion


        #region Composite Overrides

        public override Composite CombatBehavior => new ActionRunCoroutine(ctx => CreateCombat());//CreateCombat();
        public override Composite PullBehavior => new ActionRunCoroutine(ctx => CreatePull());
        public override Composite PreCombatBuffBehavior => new ActionRunCoroutine(ctx => CreateBuffs());
        public override Composite RestBehavior => new ActionRunCoroutine(ctx => CreateRest());
        public override Composite HealBehavior => new ActionRunCoroutine(ctx => CreateHeal());
        public override Composite CombatBuffBehavior => new ActionRunCoroutine(ctx => CreateBuffs());
        public override Composite PullBuffBehavior => new ActionRunCoroutine(ctx => CreatePull());

        protected abstract Task<bool> CreateCombat();
        //protected virtual Composite CreateCombat() => new ActionAlwaysFail();
        protected abstract Task<bool> CreatePull();
        protected abstract Task<bool> CreateBuffs();
        protected abstract Task<bool> CreateRest();
        protected abstract Task<bool> CreateHeal();

        #endregion

        public override void Initialize()
        {
            BotEvents.OnBotStarted += OnBotStartEvent;
            BotEvents.OnBotStopped += OnBotStopEvent;
        }

        public override void ShutDown()
        {
            BotEvents.OnBotStarted -= OnBotStartEvent;
            BotEvents.OnBotStopped -= OnBotStopEvent;
        }

        readonly Stopwatch _targettimer = new Stopwatch();
        public override void Pulse()
        {
            if (BotManager.Current.Name.Contains("BGBuddy") || BotManager.Current.Name.Contains("BGFarmer"))
            {
                if (!StyxWoW.IsInGame || !StyxWoW.IsInWorld || !Me.IsValid)
                    return;
                
                StopMoving.Pulse();

                if (PvPTargeting.TargetExists() && !Me.IsDead)
                {
                    PvPTargeting.GetInCombat();
                }

                if (Me.IsDead || Me.IsGhost)
                {
                    StopMoving.Now();
                    Me.ClearTarget();
                }

                if (Me.Combat && !Me.GotTarget && Me.IsMoving)
                {
                    //StopMoving.Now();
                    PvPTargeting.TargetClosest();
                }

                PvPTargeting.TargetPulse();

                if (Me.CurrentTarget != null && !Me.IsSafelyFacing(Me.CurrentTarget, 40))
                    WoWMovement.Face(Me.CurrentTargetGuid);

                PvPMovement.PulseMovement();

                return;
            }


            if (Me.Combat && Me.CurrentTarget != null && Me.CurrentTarget.IsDead)
            {
                Log.WriteLog("Needs Targeting, Clearing Target", Colors.DodgerBlue);
                Me.ClearTarget();
            }

            AutoTarget();

            // No moving if casting
            if (Me.IsCasting)
                return;

            // Unknown what one works if at all. 
            // Running coroutine from pulse is hard. 
            // Looking to forum for examples.
            if ((BotPoi.Current.AsObject == Me.CurrentTarget || BotPoi.Current.Type == PoiType.Kill) && (BotPoi.Current.Type != PoiType.Interact || BotPoi.Current.Type != PoiType.Harvest))
                new ActionRunCoroutine(ret => Movement.Movement.PulseMovemenTask()).ExecuteCoroutine();// This one seems to work well

            //var a = new ActionRunCoroutine(ret => Helpers.Movement.PulseMovemenTask());
            //a.ExecuteCoroutine();
            //a.Tick(null);

            //if (!a.IsRunning)
            //    a.Start(null);
            //if (a.IsRunning)
            //    a.Stop(null);
        }

        private static void AutoTarget()
        {
            if (!Me.Combat) return;

            if (Me.CurrentTarget != null && (!Me.CurrentTarget.IsDead || Me.CurrentTarget.Distance < 15)) return;

            if (!Me.GotTarget)
            {
                var players = ObjectManager.GetObjectsOfType<WoWPlayer>().Any(u => u.Distance < 30 && u.IsHostile);
                var combatmobs =
                    ObjectManager.GetObjectsOfType<WoWUnit>().OrderBy(u => u.Distance)
                        .FirstOrDefault(u => u.Combat && u.Attackable && u.Distance < 30);
                if (!players && !Me.GotTarget)
                {
                    Log.WriteLog("Targeting mobs freely, no hostile players around", Colors.DodgerBlue);
                    combatmobs?.Target();
                }
            }

            var units =
                Units.UnfriendlyUnits.Where(u => u.Aggro && u.Distance <= 30 && !u.IsPlayer)
                    .OrderBy(u => u.Distance)
                    .ThenBy(u => u.HealthPercent)
                    .ToList();
            var target = units.FirstOrDefault();

            if (target == null) return;

            Log.WriteLog("Selecting " + target.SafeName + " as Target", Colors.DodgerBlue);

            target.Target();
        }

        private static bool Validcheck()
        {
            if (Me.CurrentTarget != null)
                return false;

            if (Me.CurrentTarget != null && Me.CurrentTarget.Distance < 30)
                return true;

            if (Me.CurrentTarget != null && Me.CurrentTarget.IsDead)
                return false;

            if (Me.CurrentTarget != null && !Me.CurrentTarget.Attackable)
                return false;

            return Me.CurrentTarget != null;
        }

        private static void OnBotStartEvent(object o)
        {
            //RegisterHotkeys();
            InitializeOnce();
            //EventLog.AttachCombatLogEvent();

        }

        private static void OnBotStopEvent(object o)
        {
            //EventLog.DetachCombatLogEvent();
            //UnregisterHotkeys();
        }

        public override void OnButtonPress()
        {
            foreach (var spell in SpellManager.Spells)
            {
                Log.WriteLog(string.Format("{0} = {1},", spell.Value.Name, spell.Value.Id));
            }
        }

        private static void InitializeOnce()
        {
            //ClassSettings.Initialize();

            switch (BotManager.Current.Name)
            {
                case "LazyRaider":
                    //GeneralSettings.Instance.Movement = false;
                    break;
                case "Enyo (Buddystore)":
                    //GeneralSettings.Instance.Movement = false;
                    break;
                case "Questing":
                    //GeneralSettings.Instance.Movement = true;
                    //GeneralSettings.Instance.Targeting = true;
                    Log.WriteLog($"Movement Enabled - Bot - {BotManager.Current.Name} detected");
                    break;
                case "Akatosh Quester":
                    //GeneralSettings.Instance.Movement = true;
                    //GeneralSettings.Instance.Targeting = true;
                    Log.WriteLog($"Movement Enabled - Bot - {BotManager.Current.Name} detected");
                    break;
                case "BGBuddy":
                    //GeneralSettings.Instance.Movement = true;
                    //GeneralSettings.Instance.Targeting = true;
                    //GeneralSettings.Instance.PvP = true;
                    Log.WriteLog($"Movement Enabled - Bot - {BotManager.Current.Name} detected");
                    break;
                case "BGFarmer [Millz]":
                    //GeneralSettings.Instance.Movement = true;
                    //GeneralSettings.Instance.Targeting = true;
                    //GeneralSettings.Instance.PvP = true;
                    Log.WriteLog($"Movement Enabled - Bot - {BotManager.Current.Name} detected");
                    break;
                case "Combat Bot":
                    //GeneralSettings.Instance.Movement = true;
                    //GeneralSettings.Instance.Targeting = true;
                    Log.WriteLog($"Movement Enabled - Bot - {BotManager.Current.Name} detected");
                    break;
                case "Grind Bot":
                    //GeneralSettings.Instance.Movement = true;
                    Log.WriteLog($"Movement Enabled - Bot - {BotManager.Current.Name} detected");
                    break;
                case "Raid Bot":
                    //GeneralSettings.Instance.Movement = false;
                    break;
                case "RaidBot Improved":
                    //GeneralSettings.Instance.Movement = false;
                    break;
                default:
                    //GeneralSettings.Instance.Movement = false;
                    //GeneralSettings.Instance.Targeting = true;
                    Log.WriteLog($"Botbase - {BotManager.Current.Name} detected");
                    break;
            }

            //TalentManager.Init();
            //GeneralSettings.Instance.Save();
            Log.WriteLog("Mandalore Loaded", Colors.Orange);
        }

        #region Hooks

        //protected virtual Composite CreateCombat()
        //{
        //    return new HookExecutor("Mandalore_Combat_Root",
        //        "Root composite for Axiom combat. Rotations will be plugged into this hook.",
        //        new ActionAlwaysFail());
        //}

        //protected virtual Composite CreateBuffs()
        //{
        //    return new HookExecutor("Mandalore_Buffs_Root",
        //        "Root composite for Mandalore buffs. Rotations will be plugged into this hook.",
        //        new ActionAlwaysFail());
        //}

        //protected virtual Composite CreateRest()
        //{
        //    return new HookExecutor("Mandalore_Rest_Root",
        //        "Root composite for Mandalore Resting. Rotations will be plugged into this hook.",
        //        new ActionAlwaysFail());
        //}

        //protected virtual Composite CreatePull()
        //{
        //    return new HookExecutor("Mandalore_Pull_Root",
        //        "Root composite for Mandalore Pulling. Rotations will be plugged into this hook.",
        //        new ActionAlwaysFail());
        //}

        //protected virtual Composite CreateHeal()
        //{
        //    return new HookExecutor("Mandalore_Pull_Root",
        //        "Root composite for Mandalore Pulling. Rotations will be plugged into this hook.",
        //        new ActionAlwaysFail());
        //}

        #endregion

    }
}
