using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Styx;
using Styx.CommonBot.Coroutines;
using Styx.CommonBot.Inventory;
using Styx.WoWInternals.WoWObjects;

namespace Mandalore.Helpers
{
    class Rest
    {
        private static LocalPlayer Me = StyxWoW.Me;
        public static async Task<bool> CreateRestHp()
        {
            if (StyxWoW.Me.IsDead || StyxWoW.Me.IsGhost || Me.Mounted)
                return false;

            // Check if we even need to rest
            if (StyxWoW.Me.HealthPercent >= 70)
                return false;

            // Keep returning true if we're eating and don't have enough health yet
            if (StyxWoW.Me.HasAura("Food") && StyxWoW.Me.HealthPercent < 90)
                return true;

            // Can't eat or drink if we're swimming
            if (StyxWoW.Me.IsSwimming)
                return false;

            // Eat food if our health is less than our setting
            if (StyxWoW.Me.HealthPercent < 65)
            {
                // Check if we even have food
                if (Consumable.GetBestFood(true) == null)
                {
                    Log.WriteLog("We don't have any food");
                    return false;
                }

                Consumable.GetBestFood(true).Use();
                Log.WriteLog("Eating " + Consumable.GetBestFood(true).Name);
                //Styx.CommonBot.Rest.FeedImmediate();
                await CommonCoroutines.SleepForLagDuration();
                return true;
            }

            return false;
        }

        protected async Task<bool> CreateRestHPMP()
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
                await Spell.CoCast(SpellList.FlashofLight);
                return true;
            }

            // Can't eat or drink if we're swimming
            if (StyxWoW.Me.IsSwimming)
                return false;

            // Eat food if our health is less than our setting
            if (StyxWoW.Me.HealthPercent < 65)
            {
                // Check if we even have food
                if (Consumable.GetBestFood(true) == null)
                {
                    Log.WriteLog("We don't have any food");
                    return false;
                }

                Consumable.GetBestDrink(true).Use();
                Log.WriteLog("Eating " + Consumable.GetBestFood(true).Name);
                await CommonCoroutines.SleepForLagDuration();
                return true;
            }

            // Drink if our health is less than our setting
            if (StyxWoW.Me.ManaPercent < 50)
            {
                // Check if we even have Drink
                if (Consumable.GetBestDrink(true) == null)
                {
                    Log.WriteLog("We don't have any drinks");
                    return false;
                }

                Consumable.GetBestDrink(true).Use();
                Log.WriteLog("Drinking " + Consumable.GetBestFood(true).Name);
                await CommonCoroutines.SleepForLagDuration();
                return true;
            }

            return false;
        }
    }
}
