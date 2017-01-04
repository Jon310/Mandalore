using System.Threading.Tasks;
using Buddy.Coroutines;
using CommonBehaviors.Actions;
using Mandalore.Helpers;
using Styx;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.CommonBot.Inventory;
using Styx.TreeSharp;
using S = Mandalore.Helpers.SpellList;

namespace Mandalore.Class.Paladin
{
    class Holy : Mandalore
    {
        public override WoWClass Class
            => Me.Specialization == WoWSpec.PaladinHoly ? WoWClass.Paladin : WoWClass.None;


        protected async override Task<bool> CreateCombat()
        {
            if (!Me.Combat || Me.Mounted || !Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            await Spell.CoCast(S.HammerofJustice, Me.CurrentTarget.IsCasting && !Me.CurrentTarget.CanInterruptCurrentSpellCast);
            await Spell.CoCast(S.HolyLight, Me.HasAura(54149) && Me.HealthPercent < 85);

            await Spell.CoCast(S.TyrsDeliverance, Me.HealthPercent < 60);
            await Spell.CoCast(S.FlashofLight, Me.HealthPercent < 40);
            await Spell.CoCast(S.AvengingWrath);
            await Spell.CoCast(S.AuraMastery, Me.HealthPercent < 30);
            await Spell.CoCast(S.DivineShield, Me.HealthPercent < 20);
            await Spell.CoCast(S.DivineProtection, Me.HealthPercent < 90);



            await Spell.CoCast(S.HolyShock);
            await Spell.CoCast(S.CrusaderStrike);
            await Spell.CoCast(S.Judgment);
            await Spell.CoCast(S.Consecration, Me.CurrentTarget.IsWithinMeleeRange);

            return true;
        }



        protected async override Task<bool> CreateBuffs()
        {
            if (Me.HealthPercent < 70)
            {
                if (Me.IsMoving)
                    await CommonCoroutines.StopMoving("Buffs: Need Heal, Stop Moving");
                await Spell.CoCast(S.FlashofLight);
            }

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
            if (Me.HealthPercent < 70)
            {
                if (Me.IsMoving)
                    await CommonCoroutines.StopMoving("Heal: Need Heal, Stop Moving");
                await Spell.CoCast(S.FlashofLight);
            }

            return false;
        }

        #region RestCoroutine

        protected async override Task<bool> CreateRest()
        {
            if (Me.IsDead || SpellManager.GlobalCooldown)
                return false;

            if (!(Me.HealthPercent < 60) || Me.IsMoving || Me.IsCasting || Me.Combat || Me.HasAura("Food") ||
                Consumable.GetBestFood(true) == null)
                return false;

            Styx.CommonBot.Rest.FeedImmediate();
            return await Coroutine.Wait(1000, () => Me.HasAura("Food"));
        }

        #endregion
    }
}
