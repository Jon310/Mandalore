using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using Bots.DungeonBuddy.Helpers;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.Helpers;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace Mandalore.Helpers
{
    public static class Units
    {
        private static readonly LocalPlayer Me = StyxWoW.Me;

        #region EnemyUnits

        public static IEnumerable<WoWUnit> EnemyUnits(int maxSpellDist)
        {
            var typeWoWUnit = typeof(WoWUnit);
            var typeWoWPlayer = typeof(WoWPlayer);
            var objectList = ObjectManager.ObjectList;
            return (from t1 in objectList
                    let type = t1.GetType()
                    where type == typeWoWUnit || type == typeWoWPlayer
                    select t1 as WoWUnit into t
                    where t != null && t.Distance <= maxSpellDist && !t.IsMe && t.IsHostile
                    select t).ToList();
        }

        #endregion

        #region UnfriendlyUnits

        public static bool ResetUnfriendlyUnits = false;
        private static IEnumerable<WoWUnit> _unfriendlyUnits;

        public static IEnumerable<WoWUnit> UnfriendlyUnits
        {
            get
            {
                if (_unfriendlyUnits == null || ResetUnfriendlyUnits)
                {
                    using (StyxWoW.Memory.AcquireFrame(true))
                    {
                        _unfriendlyUnits =
                            ObjectManager.GetObjectsOfType<WoWUnit>().Where(u => u.ValidAttackUnit() && u.Distance < 40);
                        ResetUnfriendlyUnits = false;
                    }
                }

                return _unfriendlyUnits.Where(u => u.IsValid);
            }
        }

        public static IEnumerable<WoWUnit> BadGuys(int maxDist)
        {
            using (StyxWoW.Memory.AcquireFrame(true))
            {
                return ObjectManager.GetObjectsOfType<WoWUnit>().Where(u => u.ValidAttackUnit() && u.Distance < maxDist);
            }
        } 

        public static bool ValidAttackUnit(this WoWUnit p)
        {
            if (p == null || !p.IsValid)
                return false;

            if (!p.Attackable)
                return false;

            if (p.IsFriendly)
                return false;

            if (p.IsDead)
                return false;

            if (p.IsTotem && !p.IsHostile)
                return false;

            if (p.IsNonCombatPet)
                return false;

            if (!p.CanSelect)
                return false;

            if (p.IsCritter)
                return false;

            if (p.IsPlayer && !p.IsHostile)
                return false;

            if (p.IsPet && !p.IsHostile)
                return false;

            return true;
        }

        #endregion


        #region EnemyUnitsCone

        public static IEnumerable<WoWUnit> EnemyUnitsCone(WoWUnit target, IEnumerable<WoWUnit> otherUnits, float distance)
        {
            var targetLoc = target.Location;
            // most (if not all) player cone spells are 90 degrees.
            return otherUnits.Where(u => target.IsSafelyFacing(u, 90) && u.Location.Distance(targetLoc) <= distance);
        }

        #endregion



        #region EnemyUnitsSub40

        public static IEnumerable<WoWUnit> EnemyUnitsSub40 => EnemyUnits(40);

        #endregion

        #region EnemyUnitsSub10

        public static IEnumerable<WoWUnit> EnemyUnitsSub10 => EnemyUnits(10);

        #endregion

        #region EnemyUnitsSub8

        public static IEnumerable<WoWUnit> EnemyUnitsSub8 => EnemyUnits(8);

        #endregion

        #region EnemyUnitsMelee

        public static IEnumerable<WoWUnit> EnemyUnitsMelee => EnemyUnits(Me.MeleeRange.ToStringInvariant().ToInt32()/*.ToString(CultureInfo.InvariantCulture).ToInt32()*/);

        #endregion

        #region FriendlyUnitsNearTarget
        public static IEnumerable<WoWUnit> FriendlyUnitsNearTarget(float distance)
        {
            var dist = distance * distance;
            var curTarLocation = StyxWoW.Me.CurrentTarget.Location;
            return ObjectManager.GetObjectsOfType<WoWUnit>().Where(
                        p => p.IsFriendly && p.Location.DistanceSquared(curTarLocation) <= dist).ToList();
        }
        #endregion

        #region EnemyUnitsNearTarget
        public static IEnumerable<WoWUnit> EnemyUnitsNearTarget(float distance)
        {
            var dist = distance * distance;
            var curTarLocation = StyxWoW.Me.CurrentTarget.Location;
            return ObjectManager.GetObjectsOfType<WoWUnit>().Where(
                        p => p.IsHostile && p.Location.DistanceSquared(curTarLocation) <= dist).ToList();
        }
        #endregion

        #region GetPathUnits

        public static IEnumerable<WoWUnit> GetPathUnits(WoWUnit target, IEnumerable<WoWUnit> otherUnits, float distance)
        {
            var myLoc = StyxWoW.Me.Location;
            var targetLoc = target.Location;
            return otherUnits.Where(u => u.Location.GetNearestPointOnSegment(myLoc, targetLoc).Distance(u.Location) <= distance);
        }

        #endregion

        #region Auras

        public static bool HasAura(this WoWUnit unit, int auraid, int msLeft, int stacks)
        {
            var result = unit?.GetAllAuras().FirstOrDefault(a => a.CreatorGuid == StyxWoW.Me.Guid && a.SpellId == auraid && !a.IsPassive);
            if (result == null)
                return false;

            if (result.TimeLeft.TotalMilliseconds < msLeft && msLeft != 0)
                return false;

            return result.StackCount >= stacks || stacks == 0;
        }
        public static bool HasAura(this WoWUnit unit, string aura, int stacks)
        {
            return HasAura(unit, aura, stacks, null);
        }
        public static bool HasMyAura(this WoWUnit unit, int aura)
        {
            return unit.GetAllAuras().Any(a => a.SpellId == aura && a.CreatorGuid == Me.Guid);
        }
        private static bool HasAura(this WoWUnit unit, string aura, int stacks, WoWUnit creator)
        {
            return unit.GetAllAuras().Any(a => a.Name == aura && a.StackCount >= stacks && (creator == null || a.CreatorGuid == creator.Guid));
        }
        public static bool HasAura(this WoWUnit unit, string name, bool myAurasOnly)
        {
            var result = unit?.GetAllAuras().FirstOrDefault(a => a.CreatorGuid == StyxWoW.Me.Guid && a.Name == name && !a.IsPassive);
            return result != null;
        }
        public static bool HasAnyAura(this WoWUnit unit, params string[] auraNames)
        {
            var auras = unit.GetAllAuras();
            var hashes = new HashSet<string>(auraNames);
            return auras.Any(a => hashes.Contains(a.Name));
        }
        public static uint AuraTimeLeft(this WoWUnit unit, int aura)
        {
            if (!unit.IsValid)
                return 0;

            var result = unit.GetAllAuras().FirstOrDefault(a => a.CreatorGuid == StyxWoW.Me.Guid && a.SpellId == aura && !a.IsPassive);

            return result?.Duration ?? 0;
        }
        public static TimeSpan GetAuraTimeLeft(this WoWUnit onUnit, string auraName, bool fromMyAura = true)
        {
            var wantedAura =
                onUnit.GetAllAuras().FirstOrDefault(a => a != null && a.Name == auraName && a.TimeLeft > TimeSpan.Zero && (!fromMyAura || a.CreatorGuid == StyxWoW.Me.Guid));

            return wantedAura?.TimeLeft ?? TimeSpan.Zero;
        }
        public static uint GetAuraStackCount(this WoWUnit unit, string aura, bool fromMyAura = true)
        {
            if (unit == null || !unit.IsValid) return uint.MinValue;
            var s = unit.Auras.Values.FirstOrDefault(a => a.Name == aura && a.CreatorGuid == Me.Guid);
            if (s == null) return uint.MinValue;
            Log.WritetoFile(LogLevel.Diagnostic,
                $"{unit.SafeName} has {unit.Auras[aura].StackCount} stacks of {aura}");
            return s.StackCount;
        }
        public static bool HasAuraExpired(this WoWUnit u, string aura, int secs = 3, bool myAura = true)
        {
            return u.HasAuraExpired(aura, aura, secs, myAura);
        }
        public static bool HasAuraExpired(this WoWUnit u, string spell, string aura, int secs = 3, bool myAura = true)
        {
            // need to compare millisecs even though seconds are provided.  otherwise see it as expired 999 ms early because
            // .. of loss of precision
            return SpellManager.HasSpell(spell) && u.GetAuraTimeLeft(aura, myAura).TotalSeconds <= secs;
        }
        #endregion

        #region InRange

        public static bool InRange(this WoWUnit unit)
        {
            if (!unit.IsValid)
                return false;
            if (unit.Guid == Me.Guid)
                return true;
            return unit.Distance <= System.Math.Max(5f, Me.CombatReach + 1.3333334f + unit.CombatReach) || unit.IsWithinMeleeRange;
        }

        #endregion

        #region IsFriendly

        public static bool IsFriendly(this WoWUnit target)
        {
            if (!target.IsValid)
                return false;
            if (target.Guid == Me.Guid)
                return true;
            //if (HealManager.InitialList.Contains(Target.ToPlayer()))
            //    return true;
            if (Me.CurrentMap.IsArena && !Me.GroupInfo.IsInCurrentParty(target.Guid))
                return false;
            if (target.IsFriendly)
                return true;
            return target.IsPlayer && target.ToPlayer().FactionGroup == Me.FactionGroup;
        }

        #endregion

        #region Status

        public static string Status(this WoWUnit unit)
        {
            if (!unit.IsValid)
                return "Unknown";
            if (Mandalore.Me.Role == WoWPartyMember.GroupRole.Tank && unit.IsHostile)
                return unit.ThreatInfo.RawPercent + "%Threat";
            if (Mandalore.Me.Role == WoWPartyMember.GroupRole.Damage && !unit.IsFriendly)
                return System.Math.Round(Mandalore.Me.EnergyPercent) + "%Energy" + System.Math.Round(unit.HealthPercent) + "%HP's" +
                       Mandalore.Me.ComboPoints + "CP's";
            if (Mandalore.Me.Role == WoWPartyMember.GroupRole.Healer)
                return System.Math.Round(unit.HealthPercent()) + "%HP's " + System.Math.Round(Mandalore.Me.PowerPercent) + "%" +
                       Mandalore.Me.PowerType + " " + Mandalore.Me.CurrentChi + "CP's";
            return System.Math.Round(unit.HealthPercent) + "%HP's";
        }

        #endregion

        #region HealthPercent (Predicted and Current)

        public static double HealthPercent(this WoWUnit unit)
        {
            if (unit == null || !unit.IsValid)
                return double.MinValue;
            return System.Math.Max(unit.GetPredictedHealthPercent(), unit.HealthPercent);
        }

        #endregion

        #region DebuffCC
        public static bool DebuffCc(this WoWUnit target)
        {
            {

                if (!target.IsPlayer)
                {
                    return false;
                }
                if (target.Stunned)
                {
                    Log.WriteLog("Stunned!", Colors.Red);
                    return true;
                }
                if (target.Silenced)
                {
                    Log.WriteLog("Silenced", Colors.Red);
                    return true;
                }
                if (target.Dazed)
                {
                    Log.WriteLog("Dazed", Colors.Red);
                    return true;
                }

                var auras = target.GetAllAuras();

                return auras.Any(a => a.Spell != null && a.Spell.SpellEffects.Any(
                se => se.AuraType == WoWApplyAuraType.ModConfuse
                    || se.AuraType == WoWApplyAuraType.ModCharm
                    || se.AuraType == WoWApplyAuraType.ModFear
                    || se.AuraType == WoWApplyAuraType.ModPacify
                    || se.AuraType == WoWApplyAuraType.ModPacifySilence
                    || se.AuraType == WoWApplyAuraType.ModPossess
                    || se.AuraType == WoWApplyAuraType.ModStun
                ));
            }
        }
        #endregion

        #region PartyBuffs

        public static bool HasPartyBuff(this WoWUnit onunit, Stat stat)
        {
            switch (stat)
            {
                case Stat.AttackPower:
                    return onunit.HasAnyAura("Horn of Winter", "Trueshot Aura", "Battle Shout");
                case Stat.BurstHaste:
                    return onunit.HasAnyAura("Time Warp", "Ancient Hysteria", "Heroism", "Bloodlust", "Netherwinds", "Drums of Fury");
                case Stat.CriticalStrike:
                    return onunit.HasAnyAura("Leader of the Pack", "Arcane Brilliance", "Dalaran Brilliance", "Legacy of the White Tiger",
                        "Lone Wolf: Ferocity of the Raptor", "Terrifying Roar", "Fearless Roar", "Strength of the Pack", "Embrace of the Shale Spider",
                        "Still Water", "Furious Howl");
                case Stat.Haste:
                    return onunit.HasAnyAura("Unholy Aura", "Mind Quickening", "Swiftblade's Cunning", "Grace of Air", "Lone Wolf: Haste of the Hyena",
                        "Cackling Howl", "Savage Vigor", "Energizing Spores", "Speed of the Swarm");
                case Stat.Mastery:
                    return onunit.HasAnyAura("Power of the Grave", "Moonkin Aura", "Blessing of Might", "Grace of Air", "Lone Wolf: Grace of the Cat",
                        "Roar of Courage", "Keen Senses", "Spirit Beast Blessing", "Plainswalking");
                case Stat.MortalWounds:
                    return onunit.HasAnyAura("Mortal Strike", "Wild Strike", "Wound Poison", "Rising Sun Kick", "Mortal Cleave", "Legion Strike",
                        "Bloody Screech", "Deadly Bite", "Monstrous Bite", "Gruesome Bite", "Deadly Sting");
                case Stat.Multistrike:
                    return onunit.HasAnyAura("Windflurry", "Mind Quickening", "Swiftblade's Cunning", "Dark Intent", "Lone Wolf: Quickness of the Dragonhawk",
                        "Sonic Focus", "Wild Strength", "Double Bite", "Spry Attacks", "Breath of the Winds");
                case Stat.SpellPower:
                    return onunit.HasAnyAura("Arcane Brilliance", "Dalaran Brilliance", "Dark Intent", "Lone Wolf: Wisdom of the Serpent", "Still Water",
                        "Qiraji Fortitude", "Serpent's Cunning");
                case Stat.Stamina:
                    return onunit.HasAnyAura("Power Word: Fortitude", "Blood Pact", "Commanding Shout", "Lone Wolf: Fortitude of the Bear",
                        "Fortitude", "Invigorating Roar", "Sturdiness", "Savage Vigor", "Qiraji Fortitude");
                case Stat.Stats:
                    return onunit.HasAnyAura("Mark of the Wild", "Legacy of the Emperor", "Legacy of the White Tiger", "Blessing of Kings",
                        "Lone Wolf: Power of the Primates", "Blessing of Forgotten Kings", "Bark of the Wild", "Blessing of Kongs",
                        "Embrace of the Shale Spider", "Strength of the Earth");
                case Stat.Versatility:
                    return onunit.HasAnyAura("Unholy Aura", "Mark of the Wild", "Sanctity Aura", "Inspiring Presence", "Lone Wolf: Versatility of the Ravager",
                        "Tenacity", "Indomitable", "Wild Strength", "Defensive Quills", "Chitinous Armor", "Grace", "Strength of the Earth");
            }

            return false;
        }

        public enum Stat
        {
            AttackPower,
            BurstHaste,
            CriticalStrike,
            Haste,
            Mastery,
            MortalWounds,
            Multistrike,
            SpellPower,
            Stamina,
            Stats,
            Versatility
        }

        #endregion

        #region mechanic
        public static bool HasAuraWithMechanic(this WoWUnit unit, params WoWSpellMechanic[] mechanics)
        {
            var auras = unit.GetAllAuras();
            return auras.Any(a => mechanics.Contains(a.Spell.Mechanic));
        }

        public static bool IsStunned(this WoWUnit unit)
        {
            return unit.HasAuraWithMechanic(WoWSpellMechanic.Stunned, WoWSpellMechanic.Incapacitated);
        }

        public static bool IsCrowdControlled(this WoWUnit unit)
        {
            return unit.Stunned
                || unit.Rooted
                || unit.Fleeing
                || unit.HasAuraWithEffectsing(
                        WoWApplyAuraType.ModConfuse,
                        WoWApplyAuraType.ModCharm,
                        WoWApplyAuraType.ModFear,
                        WoWApplyAuraType.ModDecreaseSpeed,
                        WoWApplyAuraType.ModPacify,
                        WoWApplyAuraType.ModPacifySilence,
                        WoWApplyAuraType.ModPossess,
                        WoWApplyAuraType.ModRoot,
                        WoWApplyAuraType.ModStun);
        }

        public static bool HasAuraWithEffectsing(this WoWUnit unit, params WoWApplyAuraType[] applyType)
        {
            var hashes = new HashSet<WoWApplyAuraType>(applyType);
            return unit.Auras.Values.Any(a => a.Spell != null && a.Spell.SpellEffects.Any(se => hashes.Contains(se.AuraType)));
        }

        public static bool IsSlowed(this WoWUnit unit)
        {
            return unit.GetAllAuras().Any(a => a.Spell.SpellEffects.Any(e => e.AuraType == WoWApplyAuraType.ModDecreaseSpeed));
        }
        #endregion


    }
}
