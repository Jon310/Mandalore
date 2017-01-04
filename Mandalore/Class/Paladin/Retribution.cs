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
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using S = Mandalore.Helpers.SpellList;

namespace Mandalore.Class.Paladin
{
    internal class Retribution : Mandalore
    {
        #region Overrides

        public override WoWClass Class
            => Me.Specialization == WoWSpec.PaladinRetribution ? WoWClass.Paladin : WoWClass.None;
        
        #endregion

        private static DateTime _lastdeathndecay = DateTime.MinValue;

        protected async override Task<bool> CreateCombat()
        {
            if (!Me.Combat || Me.Mounted || !Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            await Spell.CoCast(S.Rebuke, Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast);
            await Spell.CoCast(S.HammerofJustice, Me.CurrentTarget.IsCasting && !Me.CurrentTarget.CanInterruptCurrentSpellCast);
            await Spell.CoCast(S.AvengingWrath);
            await Spell.CoCast(S.DivineShield, Me.HealthPercent < 40);
            await Spell.CoCast(S.ShieldofVengeance, Me.HealthPercent < 60);
            await Spell.CoCast(S.LayonHands, Me.HealthPercent < 15);

            await Spell.CoCast(S.Judgment);

            if (Units.EnemyUnitsMelee.Count() == 2)
            {
                await Spell.CoCast(S.DivineStorm, Me.CurrentHolyPower > 4 || Me.CurrentTarget.HasAuraExpired("Judgement", 2));
                await Spell.CoCast(S.WakeofAshes, Me.CurrentTarget.Distance < 12);
                await Spell.CoCast(S.Zeal, Spell.GetCharges(S.Zeal) > 1);
                await Spell.CoCast(S.DivineStorm);
                await Spell.CoCast(S.BladeofWrath);
                await Spell.CoCast(S.Zeal);
            }

            if (Units.EnemyUnitsMelee.Count() > 2)
            {
                await Spell.CoCast(S.DivineStorm, Me.CurrentHolyPower > 4 || Me.CurrentTarget.HasAuraExpired("Judgement", 2));
                await Spell.CoCast(S.WakeofAshes, Me.CurrentTarget.Distance < 12);
                await Spell.CoCast(S.Zeal, Spell.GetCharges(S.Zeal) > 1);
                await Spell.CoCast(S.DivineHammer);
                await Spell.CoCast(S.Zeal);
                await Spell.CoCast(S.Consecration);
            }

            await Spell.CoCast(S.TemplarsVerdict, Me.CurrentHolyPower > 4 || Me.CurrentTarget.HasAuraExpired("Judgement", 2));
            await Spell.CoCast(S.WakeofAshes, Me.CurrentTarget.Distance < 12);
            await Spell.CoCast(S.CrusaderStrike, Spell.GetCharges(S.CrusaderStrike) > 1);
            await Spell.CoCast(S.TemplarsVerdict);
            await Spell.CoCast(S.BladeofWrath);
            await Spell.CoCast(S.CrusaderStrike);

            return false;
        }

        protected async override Task<bool> CreateBuffs()
        {
            await Spell.CoCast(S.GreaterBlessingofKings, !Me.HasAura(S.GreaterBlessingofKings));
            await Spell.CoCast(S.GreaterBlessingoMight, !Me.HasAura(S.GreaterBlessingoMight));
            await Spell.CoCast(S.GreaterBlessingofWisdom, !Me.HasAura(S.GreaterBlessingofWisdom));

            return false;
        }

        protected async override Task<bool> CreatePull()
        {
            if (!Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            if (Me.Mounted && Me.CurrentTarget.Distance < 15)
                await CommonCoroutines.Dismount();

            await Spell.CoCast(S.Judgment);
            await Spell.CoCast(S.HandofReckoning);

            await CommonCoroutines.MoveTo(Me.CurrentTarget.Location);

            return false;
        }

        protected async override Task<bool> CreateHeal()
        {
            await Spell.CoCast(S.FlashofLight, Me.HealthPercent < 70);

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
                await Spell.CoCast(S.FlashofLight);
                return true;
            }

            // Can't eat or drink if we're swimming
            if (StyxWoW.Me.IsSwimming)
                return false;

            // Eat food if our health is less than our setting
            if (StyxWoW.Me.HealthPercent < 65)
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
