using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mandalore.Helpers;
using Styx;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using S = Mandalore.Helpers.SpellList;

namespace Mandalore.Class.DeathKnight
{
    class Unholy : Mandalore
    {
        public override WoWClass Class => Me.Specialization == WoWSpec.DeathKnightUnholy ? WoWClass.DeathKnight : WoWClass.None;
        readonly WoWPlayer _deadbuddy = ObjectManager.GetObjectsOfType<WoWPlayer>().FirstOrDefault(u => u.IsFriendly && u.IsDead);
        protected async override Task<bool> CreateCombat()
        {
            if (!Me.Combat || Me.Mounted || !Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            if (Me.Pet == null)
                await Spell.CastSpell("Raise Dead", Me, true);

            await Spell.CoCast(S.Outbreak, !Me.CurrentTarget.HasAura("Virulent Plague"));
            await Spell.CoCast(S.DarkTransformation);
            await Spell.CoCast(S.SummonGargoyle);
            await Spell.CoCast(S.SoulReaper, Me.CurrentTarget.HasAura("Festering Wound", 3));
            await Spell.CoCast(S.Apocalypse, Me.CurrentTarget.HasAura("Festering Wound", 8) && Me.CurrentTarget.HasAura("Soul Reaper"));

            await Spell.CoCast(S.ArmyoftheDead, Me.HealthPercent < 60);
            await Spell.CoCast(S.IceboundFortitude, Me.HealthPercent < 70);

            await Spell.CoCast(S.RaiseAlly, _deadbuddy);

            await Spell.CoCast(S.ChainsofIce, Me.CurrentTarget.IsPlayer && !Me.CurrentTarget.IsSlowed());

            await Spell.CoCast(S.MindFreeze, (Me.CurrentTarget.IsCasting || Me.CurrentTarget.IsChanneling) &&
                                                Me.CurrentTarget.CanInterruptCurrentSpellCast && Me.CurrentTarget.CurrentCastTimeLeft.TotalSeconds < 1);
            await Spell.CoCast(S.AntiMagicShell, (Me.CurrentTarget.IsCasting || Me.CurrentTarget.IsChanneling) &&
                                                Me.CurrentTarget.CanInterruptCurrentSpellCast && SpellManager.Spells["Mind Freeze"].Cooldown && Me.CurrentTarget.CurrentCastTimeLeft.TotalSeconds < 1);

            await Spell.CoCast(S.DeathStrike, Me.HasAura("Dark Succor"));

            if (SpellManager.Spells["Apocalypse"].Cooldown && !SpellManager.Spells["Soul Reaper"].Cooldown &&
                Me.CurrentTarget.HasAura("Festering Wound", 3))
            {
                await Spell.CoCast(S.SoulReaper);
                await Spell.CoCast(S.ScourgeStrike);
            }

            if (await Spell.CastOnGround(S.DeathandDecay, Me.CurrentTarget, Units.EnemyUnitsSub10.Count() > 2))
                return true;

            await Spell.CoCast(S.DeathCoil, Me.CurrentRunicPower > 80);
            await Spell.CoCast(S.FesteringStrike, Me.CurrentTarget.GetAuraStackCount("Festering Wound") < 3);
            await Spell.CoCast(S.ScourgeStrike, Me.CurrentTarget.HasAura("Festering Wound", 3));
            await Spell.CoCast(S.DeathStrike, Me.CurrentRunes == 0 && Me.HealthPercent < 80);
            await Spell.CoCast(S.DeathCoil, Me.CurrentRunes == 0 || Me.HasAura("Sudden Doom"));
            
            return false;
        }

        protected async override Task<bool> CreatePull()
        {
            if (!Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            if (Me.CurrentTarget.Distance < 10)
                await CommonCoroutines.Dismount();

            await Spell.CoCast(S.DeathGrip, Me.CurrentTarget.Distance > 15);
            await Spell.CoCast(S.Outbreak);
            await CommonCoroutines.MoveTo(Me.CurrentTarget.Location);

            return false;
        }

        protected async override Task<bool> CreateBuffs()
        {
            if (Me.Pet == null)
                await Spell.CastSpell("Raise Dead", Me, true);
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
    }
}
