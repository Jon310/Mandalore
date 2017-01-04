using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Mandalore.Helpers;
using Styx;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.Pathing;
using Styx.WoWInternals;
using S = Mandalore.Helpers.SpellList;

namespace Mandalore.Class.DemonHunter
{
    class Havoc : Mandalore
    {
        public override WoWClass Class => Me.Specialization == WoWSpec.DemonHunterHavoc ? WoWClass.DemonHunter : WoWClass.None;
        DateTime _momentum;
        protected async override Task<bool> CreateCombat()
        {
            if (!Me.Combat || Me.Mounted || !Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            if (await Spell.CoCast(S.ConsumeMagic, (Me.CurrentTarget.IsCasting || Me.CurrentTarget.IsChanneling) &&
                                                Me.CurrentTarget.CanInterruptCurrentSpellCast && Me.CurrentTarget.CurrentCastTimeLeft.TotalSeconds < 1))
                return true;

            if (await Spell.CoCast(S.ChaosNova, (Me.CurrentTarget.IsCasting || Me.CurrentTarget.IsChanneling) && SpellManager.Spells["Consume Magic"].Cooldown &&
                                                Me.CurrentTarget.CanInterruptCurrentSpellCast && Me.CurrentTarget.CurrentCastTimeLeft.TotalSeconds < 1))
                return true;

            if (await Spell.CoCast(S.FelRush, Spell.GetCharges(S.FelRush) >= 2 && Me.CurrentFury < 75 && _momentum.AddSeconds(4) < DateTime.Now))
            {
                _momentum = DateTime.Now;
                return true;
            }

            await Spell.CoCast(S.SpectralSight, Units.EnemyUnitsSub40.Any(u => u.Class == WoWClass.Rogue || u.Class == WoWClass.Druid));

            //await VengRet();
            //if (await Spell.CoCast(S.VengefulRetreat, _momentum.AddSeconds(4) < DateTime.Now && Spell.GetCharges(S.FelRush) <= 1))
            //{
            //    _momentum = DateTime.Now;
            //    return true;
            //}

            if (await Spell.CastOnGround(S.Metamorphosis, Me.CurrentTarget, Me.CurrentFury > 75))
                return true;


            if (Me.CurrentTarget.Distance > 10 && Me.CurrentTarget.Distance < 25 && Me.IsFacing(Me.CurrentTarget))
            {
                if (await Spell.CoCast(S.FelRush))
                {
                    _momentum = DateTime.Now;
                }
            }
            
            await Spell.CoCast(S.Blur, Me.HealthPercent < 70);
            await Spell.CoCast(S.Darkness, Me.HealthPercent < 60);

            await Spell.CoCast(S.FuryoftheIllidari);//
            await Spell.CoCast(S.FelBarrage, Spell.GetCharges(S.FelBarrage) >= 5 && _momentum.AddSeconds(4) > DateTime.Now);//
            await Spell.CoCast(S.FelEruption);
            await Spell.CoCast(S.FelBlade, Me.CurrentFury < 70);
            await Spell.CoCast(S.ThrowGlaive, _momentum.AddSeconds(4) > DateTime.Now);//
            await Spell.CoCast(S.EyeBeam);//
            await Spell.CoCast(S.BladeDance, Units.EnemyUnitsSub10.Count() >= 2);
            await Spell.CoCast(S.ChaosStrike, _momentum.AddSeconds(4) > DateTime.Now || Me.CurrentFury > 75);//
            await Spell.CoCast(S.FelBarrage, Spell.GetCharges(S.FelBarrage) >= 4 && _momentum.AddSeconds(4) > DateTime.Now);//
            await Spell.CoCast(S.DemonsBite);//

            return false;
        }

        protected async override Task<bool> CreatePull()
        {
            if (!Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            if (Me.CurrentTarget.Distance < 10)
                await CommonCoroutines.Dismount();

            await Spell.CoCast(S.ThrowGlaive);

            if (await Spell.CoCast(S.FelRush))
            {
                _momentum = DateTime.Now;
                return true;
            }

            await CommonCoroutines.MoveTo(Me.CurrentTarget.Location);

            return false;
        }

        protected async override Task<bool> CreateBuffs()
        {
            await CreateCombat();
            return false;
        }

        protected async override Task<bool> CreateRest()
        {
            return await Helpers.Rest.CreateRestHp();
        }

        protected async override Task<bool> CreateHeal()
        {
            await CreateCombat();
            return false;
        }

        private async Task<bool> VengRet()
        {
            if (SpellManager.CanCast(S.VengefulRetreat) && _momentum.AddSeconds(4) < DateTime.Now &&
                Spell.GetCharges(S.FelRush) <= 1)
            {
                Me.SetFacing(new Vector3(-Me.CurrentTarget.X, -Me.CurrentTarget.Y, Me.Z));
                await Spell.CoCast(S.VengefulRetreat);
                _momentum = DateTime.Now;
            }

            return false;
        } 
    }
}
