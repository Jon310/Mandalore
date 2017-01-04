using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mandalore.Helpers;
using Styx;
using Styx.CommonBot.Coroutines;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using S = Mandalore.Helpers.SpellList;

namespace Mandalore.Class.Warlock
{
    class Affliction : Mandalore
    {

        #region Overrides
        public override WoWClass Class => Me.Specialization == WoWSpec.WarlockAffliction ? WoWClass.Warlock : WoWClass.None;
        #endregion

        private bool _burnphase;
        protected async override Task<bool> CreateCombat()
        {
            if (!Me.Combat || Me.Mounted || !Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            if (Me.Pet == null)
            {
                await Spell.CoCast(S.SummonDoomguard);
                await CommonCoroutines.SleepForLagDuration();
            }

            await Spell.CoCast(S.MortalCoil, Me.HealthPercent < 89 ||
                                            ((Me.CurrentTarget.IsCasting || Me.CurrentTarget.IsChanneling) && Me.CanInterruptCurrentSpellCast && Me.CurrentTarget.CurrentCastTimeLeft.TotalSeconds < 1));
            await Spell.CoCast(S.CommandDemon, (Me.CurrentTarget.IsCasting || Me.CurrentTarget.IsChanneling) && Me.CanInterruptCurrentSpellCast && Me.CurrentTarget.CurrentCastTimeLeft.TotalSeconds < 1);
            await Spell.CoCast(S.UnendingResolve, Me.HealthPercent < 60);
            await Spell.CoCast(S.DrainLife, Me.HealthPercent < 50);
            //await Spell.CastSpell("Drain Life", Me.CurrentTarget, Me.HealthPercent < 50);

            await Spell.CoCast(S.PhantomSingularity);
            await Spell.CoCast(S.ReapSouls, Me.HasAura("Tormented Souls", 3) && !Me.HasAura("Deadwind Harvester"));

            await Spell.CoCast(S.SeedofCorruption, Units.EnemyUnitsNearTarget(10).Count(u => u.HasAura("Corruption")) > 5);

            if (Me.GetPowerInfo(WoWPowerType.SoulShards).Current == Me.GetPowerInfo(WoWPowerType.SoulShards).Max && Me.HasAura("Deadwind Harvester"))
            {
                if (_burnphase == false)
                    Log.WriteLog("Setting _burnphase to true");
                _burnphase = true;
            }
            if (Me.GetPowerInfo(WoWPowerType.SoulShards).Current == 0)
            {
                if (_burnphase == true)
                    Log.WriteLog("Setting _burnphase to false");
                _burnphase = false;
            }
            
            //await Spell.CoCast(S.UnstableAffliction, !Me.CurrentTarget.HasAura("Unstable Affliction"));
            await Spell.CoCast(S.Agony, Me.CurrentTarget.HasAuraExpired("Agony"));
            await Spell.CoCast(S.Corruption, !Me.CurrentTarget.HasAura("Corruption"));
            await Spell.CoCast(S.SiphonLife, Me.CurrentTarget.HasAuraExpired("Siphon Life"));
            await Spell.CoCast(S.Haunt);

            // AOE DOT
            if (await Spell.CoCast(S.SiphonLife, NeedsDots(), NeedsDots() != null && !NeedsDots().HasAura("Siphon Life")))
            {
                Log.WriteLog("Multi Dotting " + NeedsDots().Name + " with Siphon Life");
                return true;
            }
            if (await Spell.CoCast(S.Agony, NeedsDots(), NeedsDots() != null && !NeedsDots().HasAura("Agony")))
            {
                Log.WriteLog("Multi Dotting " + NeedsDots().Name + " with Agony");
                return true;
            }
            if (await Spell.CoCast(S.Corruption, NeedsDots(), NeedsDots() != null && !NeedsDots().HasAura("Corruption")))
            {
                Log.WriteLog("Multi Dotting " + NeedsDots().Name + " with Corruption");
                return true;
            }

            await Spell.CoCast(S.UnstableAffliction, _burnphase);

            await Spell.CoCast(S.LifeTap, DottedUp(Me.CurrentTarget) && Me.HealthPercent > 70 && Me.ManaPercent < 60);

            await Spell.CoCast(S.DrainLife, Me.CurrentTarget, true, !DottedUp(Me.CurrentTarget));
            await Spell.CoCast(S.DrainSoul, Me.CurrentTarget, true, !DottedUp(Me.CurrentTarget));
            return false;
        }

        protected async override Task<bool> CreatePull()
        {
            if (Me.CurrentTarget.Distance < 30)
                await CommonCoroutines.Dismount();

            await Spell.CoCast(S.Agony, Me.CurrentTarget.HasAuraExpired("Agony"));
            await Spell.CoCast(S.Corruption, Me.CurrentTarget.HasAuraExpired("Corruption"));
            await Spell.CoCast(S.SiphonLife, Me.CurrentTarget.HasAuraExpired("Siphon Life"));

            await CommonCoroutines.MoveTo(Me.CurrentTarget.Location);

            return false;
        }

        protected async override Task<bool> CreateBuffs()
        {
            if (Me.Pet != null) return false;
            await Spell.CoCast(S.SummonDoomguard);
            await CommonCoroutines.SleepForLagDuration();
            return false;
        }

        protected async override Task<bool> CreateRest()
        {
            if (!Me.Combat)
                _burnphase = false;
            return await Helpers.Rest.CreateRestHp();
        }

        protected async override Task<bool> CreateHeal()
        {
            return false;
        }

        private static WoWUnit NeedsDots()
        {
            var dottar = Me.CurrentTarget.IsPlayer ? ObjectManager.GetObjectsOfType<WoWPlayer>().OrderBy(u => u.Distance).First(u => !DottedUp(u) && u.Distance < 40).ToUnit() 
                                                   : ObjectManager.GetObjectsOfType<WoWUnit>().OrderBy(u => u.Distance).First(u => !DottedUp(u) && u.Distance < 40 && !u.IsPlayer);

            return dottar;
        }

        private static bool DottedUp(WoWUnit target)
        {
            return !target.HasAura("Unstable Affliction") &&
                   target.HasAuraExpired("Agony") &&
                   !target.HasAura("Corruption") &&
                   target.HasAuraExpired("Siphon Life");
        }

        
    }
}
