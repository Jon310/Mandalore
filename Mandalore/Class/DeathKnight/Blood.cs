using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buddy.Coroutines;
using CommonBehaviors.Actions;
using Mandalore.Helpers;
using Styx;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.CommonBot.Inventory;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using S = Mandalore.Helpers.SpellList;

namespace Mandalore.Class.DeathKnight
{
    class Blood : Mandalore
    {
        #region Overrides
        public override WoWClass Class => Me.Specialization == WoWSpec.DeathKnightBlood? WoWClass.DeathKnight : WoWClass.None;


        #endregion

        static DateTime _lastdeathndecay = DateTime.MinValue;
        protected async override Task<bool> CreateCombat()
        {
            if (!Me.Combat || Me.Mounted || !Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            //if (await PullMore.PullMoreMobs())
            //    return false;

            await Spell.CoCast(S.AntiMagicShell, Me.CurrentTarget.IsCasting && !Me.CurrentTarget.CanInterruptCurrentSpellCast);
            await Spell.CoCast(S.DancingRuneWeapon, Me.HealthPercent < 80);
            await Spell.CoCast(S.DeathStrike, Me.HealthPercent < 60);
            await Spell.CoCast(S.VampiricBlood, Me.HealthPercent < 50);
            await Spell.CoCast(S.Bonestorm, Me.RunicPowerPercent > 60 && Units.EnemyUnitsSub10.Count() > 2);

            await Spell.CoCast(S.WraithWalk, Me.Rooted);

            await Spell.CoCast(S.MindFreeze, Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast && Me.CurrentTarget.Distance < 15);
            await Spell.CoCast(S.Asphyxiate, Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast && Me.CurrentTarget.Distance < 20);

            await Spell.CoCast(S.DeathsCaress, Me.CurrentTarget.Distance > 15);
            await Spell.CoCast(S.DeathGrip, Me.CurrentTarget.Distance > 15);
            await Spell.CoCast(S.GorefiendsGrasp, SpellManager.Spells["Death Grip"].Cooldown && Me.CurrentTarget.Distance > 15);

            await Spell.CoCast(S.BloodBoil, Me.CurrentTarget.HasAura("Blood Plague"));
            await Spell.CoCast(S.Consumption);
            
            if (await Spell.CastOnGround(S.DeathandDecay, Me.CurrentTarget, Me.HasAura("Crimson Scourge")))
            {
                _lastdeathndecay = DateTime.Now;
                return true;
            }

            await Spell.CoCast(S.Marrowrend, Me.GetAuraStackCount("Bone Shield") <= 5);
            await Spell.CoCast(S.DeathStrike, Me.RunicPowerPercent > 60);
            await Spell.CoCast(S.BloodBoil, Me.GetAuraStackCount("Bone Shield") <= 4);


            if (Me.GetAuraStackCount("Bone Shield") >= 4)
            {
                if (await Spell.CastOnGround(S.DeathandDecay, Me.CurrentTarget, _lastdeathndecay.AddSeconds(10) > DateTime.Now))
                {
                    _lastdeathndecay = DateTime.Now;
                    return true;
                }
                await Spell.CoCast(S.HeartStrike);
            }

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
                await Spell.CoCast(S.DeathGrip);
            }

            if (!Me.IsAutoAttacking)
                Me.ToggleAttack();

            await Spell.CoCast(S.DeathsCaress, Me.CurrentTarget.Distance > 15);
            //await Spell.CoCast(S.DeathGrip, Me.CurrentTarget.Distance > 15);
            //await Spell.CoCast(S.GorefiendsGrasp, SpellManager.Spells["Death Grip"].Cooldown && Me.CurrentTarget.Distance > 15);

            await CommonCoroutines.MoveTo(Me.CurrentTarget.Location);

            return false;
        }

        protected async override Task<bool> CreateBuffs()
        {


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
            return false;
        }
    }
}
