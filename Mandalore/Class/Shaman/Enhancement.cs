using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buddy.Coroutines;
using CommonBehaviors.Actions;
using Mandalore.Helpers;
using Styx;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.CommonBot.POI;
using Styx.TreeSharp;
using S = Mandalore.Helpers.SpellList;

namespace Mandalore.Class.Shaman
{
    class Enhancement : Mandalore
    {

        #region Overrides
        public override WoWClass Class => Me.Specialization == WoWSpec.ShamanEnhancement ? WoWClass.Shaman : WoWClass.None;



        #endregion

        protected async override Task<bool> CreateCombat()
        {
            if (!Me.Combat || Me.Mounted || !Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            await Spell.CoCast(S.AstralShift, Me.HealthPercent < 60);
            await Spell.CoCast(S.HealingSurge, Me.CurrentMaelstrom > 20 && Me.HealthPercent < 70);

            await Spell.CoCast(S.WindShear, (Me.CurrentTarget.IsCasting || Me.CurrentTarget.IsChanneling) && Me.CanInterruptCurrentSpellCast && Me.CurrentTarget.CurrentCastTimeLeft.TotalSeconds < 1);

            await Spell.CastOnGround(S.LightningSurgeTotem, Me.CurrentTarget, Units.EnemyUnitsSub8.Count() > 3);
            await Spell.CoCast(S.FeralLunge, Me.CurrentTarget.Distance > 13);
            await Spell.CoCast(S.EarthgrabTotem, !Me.CurrentTarget.IsSlowed() && Units.EnemyUnitsSub8.Count() > 3 && Me.CurrentTarget.IsPlayer);
            await Spell.CoCast(S.Frostbrand, !Me.CurrentTarget.IsSlowed() && Me.CurrentTarget.IsPlayer);
            
            await Spell.CoCast(S.Boulderfist, !Me.HasAura("Landslide"));
            await Spell.CoCast(S.Flametongue, !Me.HasAura("Flametongue"));
            await Spell.CoCast(S.Windsong);
            await Spell.CoCast(S.Ascendance);

            await Spell.CoCast(S.DoomWinds, Me.HasAura("Flametongue"));
            await Spell.CoCast(S.FeralSpirit);

            await Spell.CoCast(S.FuryofAir, Me.HasAura("Fury of Air"));
            await Spell.CoCast(S.Stormstrike);
            await Spell.CoCast(S.Windstrike);
            await Spell.CoCast(S.Boulderfist, Spell.GetCharges(S.Boulderfist) >= 1 && Spell.GetCooldownLeft(S.Boulderfist).TotalSeconds < 2);
            await Spell.CoCast(S.Flametongue, Me.HasAuraExpired("Flametongue", 4));
            await Spell.CoCast(S.CrashLightning, Me.CurrentMaelstrom > 80);
            await Spell.CoCast(S.LavaLash, Me.CurrentMaelstrom > 90);
            await Spell.CoCast(S.Boulderfist);
            await Spell.CoCast(S.Flametongue);


            return false;
        }

        protected async override Task<bool> CreatePull()
        {
            if (!Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            if (Me.CurrentTarget.Distance < 10)
                await CommonCoroutines.Dismount();

            if (Me.CurrentTarget.IsFlying)
            {
                await CommonCoroutines.StopMoving();
                Me.SetFacing(Me.CurrentTarget);
                await Spell.CoCast(S.LightningBolt);

            }

            await Spell.CoCast(S.FeralLunge, Me.CurrentTarget.Distance > 13);

            await Spell.CoCast(S.LightningBolt);

            await Spell.CoCast(S.Stormstrike);
            await Spell.CoCast(S.Windstrike);

            await Spell.CoCast(S.Boulderfist);
            await Spell.CoCast(S.Flametongue);

            if ((Me.Combat || Me.CurrentTarget.Distance < 15) && BotPoi.Current.Type == PoiType.Kill)
                await CommonCoroutines.MoveTo(Me.CurrentTarget.Location);

            return false;
        }

        protected async override Task<bool> CreateBuffs()
        {

                await Spell.CoCast(S.FlashofLight, Me.HealthPercent < 55);


            return false;
        }

        protected async override Task<bool> CreateHeal()
        {


            return false;
        }

        #region RestCoroutine

        protected async override Task<bool> CreateRest()
        {
            if (StyxWoW.Me.IsDead || StyxWoW.Me.IsGhost || Me.Mounted)
                return false;

            // Check if we even need to rest
            if (StyxWoW.Me.HealthPercent >= 65 && StyxWoW.Me.ManaPercent >= 30)
                return false;

            // Keep returning true if we're eating and don't have enough health yet
            if (StyxWoW.Me.HasAura("Food") && StyxWoW.Me.HealthPercent < 90)
                return true;

            // Keep returning true if we're drinking and don't have enough mana yet
            if (StyxWoW.Me.HasAura("Drink") && StyxWoW.Me.ManaPercent < 90)
                return true;

            // Flash of Light if we have more MP than setting but need a heal
            if (StyxWoW.Me.HealthPercent <= 65 && StyxWoW.Me.ManaPercent > 50)
            {
                await Spell.CoCast(S.HealingSurge);
                return true;
            }

            // Can't eat or drink if we're swimming
            if (StyxWoW.Me.IsSwimming)
                return false;

            // Eat food if our health is less than our setting
            if (StyxWoW.Me.HealthPercent < 70)
            {
                // Check if we even have food
                if (Styx.CommonBot.Rest.NoFood)
                {
                    Log.WriteLog("We don't have any food");
                    return false;
                }

                Styx.CommonBot.Rest.FeedImmediate();
                await CommonCoroutines.SleepForLagDuration();
                return true;
            }

            // Drink if our health is less than our setting
            if (StyxWoW.Me.ManaPercent < 50)
            {
                // Check if we even have Drink
                if (Styx.CommonBot.Rest.NoDrink)
                {
                    Log.WriteLog("We don't have any drinks");
                    return false;
                }

                Styx.CommonBot.Rest.DrinkImmediate();
                await CommonCoroutines.SleepForLagDuration();
                return true;
            }

            return false;
        }

        #endregion

    }
}
