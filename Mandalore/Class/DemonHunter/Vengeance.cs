using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Mandalore.Helpers;
using Styx;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.CommonBot.Inventory;
using Styx.WoWInternals.WoWObjects;
using S = Mandalore.Helpers.SpellList;

namespace Mandalore.Class.DemonHunter
{
    class Vengeance : Mandalore
    {
        public override WoWClass Class => Me.Specialization == WoWSpec.DemonHunterVengeance ? WoWClass.DemonHunter : WoWClass.None;
        static DateTime _lastDemonSpikes = DateTime.MinValue;
        protected async override Task<bool> CreateCombat()
        {
            if (!Me.Combat || Me.Mounted || !Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            //if (await PullMore.PullMoreMobs())
            //    return false;

            await Spell.CoCast(S.DemonSpikes, Me.HealthPercent < 90 || Spell.GetCharges(S.DemonSpikes) >= 2);//
            await Spell.CoCast(S.Metamorphosis, Me.HealthPercent < 60);//
            await Spell.CoCast(S.EmpowerWards, Me.CurrentTarget.IsCasting && (SpellManager.Spells["Consume Magic"].Cooldown || !Me.CurrentTarget.CanInterruptCurrentSpellCast));

            if (await Spell.CoCast(S.ConsumeMagic, (Me.CurrentTarget.IsCasting || Me.CurrentTarget.IsChanneling) && 
                                                Me.CurrentTarget.CanInterruptCurrentSpellCast && Me.CurrentTarget.CurrentCastTimeLeft.TotalSeconds < 1))
                return true;

            if (await Spell.CastOnGround(S.SigilofSilence, Me.CurrentTarget, (Me.CurrentTarget.IsCasting || Me.CurrentTarget.IsChanneling) && 
                                                (Me.CurrentTarget.IsPlayer || Units.EnemyUnitsSub10.Count() >= 3) && 
                                                Me.CurrentTarget.CurrentCastTimeLeft.TotalSeconds < 1))
                return true;

            if (await Spell.CastOnGround(S.SigilofMisery, Me.CurrentTarget, (Me.CurrentTarget.IsCasting || Me.CurrentTarget.IsChanneling) &&
                                                (Me.CurrentTarget.IsPlayer || Units.EnemyUnitsSub10.Count() >= 3) &&
                                                Me.CurrentTarget.CurrentCastTimeLeft.TotalSeconds < 1))
                return true;

            await Spell.CoCast(S.FieryBrand);
            await Spell.CoCast(S.SoulCarver);//
            await Spell.CastSpell("Fel Devastation", Me.CurrentTarget, true);
            await Spell.CastSpell("Soul Cleave", Me.CurrentTarget, Me.CurrentPain > 80);
            await Spell.CoCast(S.ImmolationAura);//
            await Spell.CastSpell("Felblade", Me.CurrentTarget, true);
            await Spell.CastSpell("Fel Eruption", Me.CurrentTarget, true);
            await Spell.CastSpell("Spirit Bomb", Me.CurrentTarget, !Me.CurrentTarget.HasAura("Frailty"));
            await Spell.CastSpell("Shear", Me.CurrentTarget, Me.HasAura("Blade Turning"));
            await Spell.CastSpell("Fracture", Me.CurrentTarget, Me.CurrentPain > 60);

            if (await Spell.CastOnGround(S.SigilofFlame, Me.CurrentTarget, true))//
                return true;
            if (await Spell.CastOnGround(S.InfernalStrike, Me.CurrentTarget, SpellManager.HasSpell("Flame Crash")))//
                return true;

            await Spell.CastSpell("Shear", Me.CurrentTarget, true);
            

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
                await Spell.CoCast(S.ThrowGlaive);
            }

            if (await Spell.CastOnGround(S.InfernalStrike, Me.CurrentTarget, true))
                return true;

            await Spell.CastSpell("Throw Glaive", Me.CurrentTarget, true);

            await CommonCoroutines.MoveTo(Me.CurrentTarget.Location);

            return false;
        }

        protected async override Task<bool> CreateBuffs()
        {
            await CreateCombat();
            return false;
        }

        #region RestCoroutine

        protected async override Task<bool> CreateRest()
        {
            return await Helpers.Rest.CreateRestHp();
        }

        #endregion

        protected async override Task<bool> CreateHeal()
        {
            await CreateCombat();
            return false;
        }
    }
}
