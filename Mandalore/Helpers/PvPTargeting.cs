using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Styx;
using Styx.Common;
using Styx.Common.Helpers;
using Styx.CommonBot.POI;
using Styx.Pathing;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace Mandalore.Helpers
{
    class PvPTargeting
    {
        private static readonly Color TargetColor = Colors.Tomato;
        private static readonly LocalPlayer Me = StyxWoW.Me;
        private static WoWUnit _target;
        private static WoWUnit _currentTarget = StyxWoW.Me;
        private static string _currentTargetType;
        private static readonly Dictionary<string, int> TargetList = new Dictionary<string, int>();

        private readonly static WaitTimer TargetTimer = new WaitTimer(TimeSpan.FromMilliseconds(10000));

        public static bool TargetExists()
        {
            var playerNear = ObjectManager.GetObjectsOfType<WoWUnit>().Where(
                                u => u.IsHostile && u.DistanceSqr <= 55 * 55 && u.IsPlayer && u.InLineOfSight).OrderBy(u => u.Distance).
                                FirstOrDefault();
            if (playerNear == null || StyxWoW.Me.IsActuallyInCombat) return false;
            _currentTarget = playerNear;
            return true;
        }

        public static void GetInCombat()
        {
            Logging.Write(TargetColor, "Trying to Force Combat. Switching to " + _target.Name + "!");
            Navigator.MoveTo(_currentTarget.Location);
            BotPoi.Current = new BotPoi(_currentTarget, PoiType.Kill);
            _currentTarget.Target();
        }

        public static bool TargetPulse()
        {
            return Targeting();
        }

        private static bool Targeting()
        {
            //Reset Timer
            if (TargetTimer.IsFinished)
            {
                TargetTimer.Reset();
            }
            else
            {
                return false;
            }
            //Clear Target
            _target = Me.CurrentTarget;
            if (Me.CurrentTarget != null)
            {
                if (_target.Mounted) return false;
                if (!_target.IsHostile) return false;
                if (!_target.Attackable) return false;
            }
            if (Me.CurrentTarget == null || !Me.CurrentTarget.IsAlive || _currentTarget == Me || _currentTarget == null)
            {
                _currentTarget = Me;
                _currentTargetType = "";
            }
            //Add list as Needed
            if (TargetList.Count < 1)
                TargetDictionary();
            //Sort the List
            var sorted = (from target in TargetList orderby target.Value descending select target).ToList();
            //Targetting
            foreach (var target in sorted)
            {
                if (target.Key == "Closest" && TargetClosest())
                {
                    _currentTargetType = "Closest";
                    return true;
                }
                if (target.Key == "FlagCarriers" && TargetFlagCarrier())
                {
                    _currentTargetType = "FlagCarriers";
                    return true;
                }
                if (target.Key == "OrbCarriers" && TargetOrbCarrier())
                {
                    _currentTargetType = "OrbCarriers";
                    return true;
                }
                if (target.Key == "Healer" && TargetHealers())
                {
                    _currentTargetType = "Healer";
                    return true;
                }
                if (target.Key == "LowHealth" && TargetLowHealth())
                {
                    _currentTargetType = "LowHealth";
                    return true;
                }
                if (target.Key == "Undergeared" && TargetUndergeared())
                {
                    _currentTargetType = "Undergeared";
                    return true;
                }
                if (target.Key != "Totems" || !TargetTotems()) continue;
                _currentTargetType = "Totems";
                return true;
            }
            return false;
        }

        private static void TargetDictionary()
        {
            //Priorities
            TargetList.Add("Closest", Convert.ToInt32(40));
            TargetList.Add("Demolishers", Convert.ToInt32(80));
            TargetList.Add("FlagCarriers", Convert.ToInt32(90));
            TargetList.Add("OrbCarriers", Convert.ToInt32(90));
            TargetList.Add("Healer", Convert.ToInt32(70));
            TargetList.Add("LowHealth", Convert.ToInt32(60));
            TargetList.Add("Undergeared", Convert.ToInt32(40));
            TargetList.Add("Totems", Convert.ToInt32(75));

        }

        public static bool TargetClosest()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (_currentTargetType == "Closest" && StyxWoW.Me.CurrentTarget.IsAlive
                        && StyxWoW.Me.CurrentTarget.Distance <= 15
                        && _currentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(_currentTarget, PoiType.Kill);
                        _target.Target();
                        return true;
                    }
                }

                //Closest!
                _target = ClosestEnemy();
                if (_target == null) return false;
                if (_target.Guid == _currentTarget.Guid) return false;
                Logging.Write(TargetColor, "Closest Enemy Spotted! Switching to " + _target.Name + "!");
                BotPoi.Current = new BotPoi(_target, PoiType.Kill);
                _currentTarget = _target;
                _target.Target();
                return true;
            }
        }

        private static bool TargetFlagCarrier()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (_currentTargetType == "FlagCarriers" && StyxWoW.Me.CurrentTarget.IsAlive
                        && StyxWoW.Me.CurrentTarget.Distance <= 30
                        && _currentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(_currentTarget, PoiType.Kill);
                        _target.Target();
                        return true;
                    }
                }

                // If there is a flag carrier return the flag carrier
                if (StyxWoW.Me.IsHorde) { _target = EnemyHordeFlagCarrier(); }
                if (StyxWoW.Me.IsAlliance) { _target = EnemyAllianceFlagCarrier(); }
                if (_target == null) return false;
                if (_target.Guid == _currentTarget.Guid) return false;
                Logging.Write(TargetColor, "Flag Carrier Spotted! Switching to " + _target.Name + "!");
                BotPoi.Current = new BotPoi(_target, PoiType.Kill);
                _currentTarget = _target;
                _target.Target();
                return true;
            }
        }

        private static bool TargetOrbCarrier()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (_currentTargetType == "OrbCarriers" && StyxWoW.Me.CurrentTarget.IsAlive
                        && StyxWoW.Me.CurrentTarget.Distance <= 30
                        && _currentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(_currentTarget, PoiType.Kill);
                        _target.Target();
                        return true;
                    }
                }

                // If there is a Orb carrier return the flag carrier
                _target = EnemyOrbCarrier();
                if (_target == null) return false;
                if (_target.Guid == _currentTarget.Guid) return false;
                Logging.Write(TargetColor, "Orb Carrier Spotted! Switching to " + _target.Name + "!");
                BotPoi.Current = new BotPoi(_target, PoiType.Kill);
                _currentTarget = _target;
                _target.Target();
                return true;
            }
        }

        private static bool TargetHealers()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (_currentTargetType == "Healer" && StyxWoW.Me.CurrentTarget.IsAlive
                        && StyxWoW.Me.CurrentTarget.Distance <= 30
                        && _currentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(_currentTarget, PoiType.Kill);
                        _target.Target();
                        return true;
                    }
                }
                _target = ValidTarget("Healer", 30);
                //Target = EnemyHealer();
                if (_target == null) return false;
                if (_target.Guid == _currentTarget.Guid) return false;
                Logging.Write(TargetColor, "Healer Spotted!. Switching to " + _target.Name + "!");
                BotPoi.Current = new BotPoi(_target, PoiType.Kill);
                _currentTarget = _target;
                _target.Target();
                return true;
            }
        }

        private static bool TargetLowHealth()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (_currentTargetType == "LowHealth" && StyxWoW.Me.CurrentTarget.IsAlive
                        && StyxWoW.Me.CurrentTarget.Distance <= 15
                        && _currentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(_currentTarget, PoiType.Kill);
                        _target.Target();
                        return true;
                    }
                }
                // Lowest Health Enemy
                _target = EnemyLowestHealth();
                if (_target == null) return false;
                if (_target.Guid == _currentTarget.Guid) return false;
                Logging.Write(TargetColor, "Low Health Spotted!. Switching to " + _target.Name + "!");
                BotPoi.Current = new BotPoi(_target, PoiType.Kill);
                _currentTarget = _target;
                _target.Target();
                return true;
            }
        }

        private static bool TargetUndergeared()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (_currentTargetType == "Undergeared" && StyxWoW.Me.CurrentTarget.IsAlive
                        && StyxWoW.Me.CurrentTarget.Distance <= 20
                        && _currentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(_currentTarget, PoiType.Kill);
                        _target.Target();
                        return true;
                    }
                }
                // Target Newbie
                _target = EnemyUndergeared();
                if (_target == null) return false;
                if (StyxWoW.Me.CurrentTarget != null) return false;
                Logging.Write(TargetColor, "Lowest Overall Health Spotted!. Switching to " + _target.Name + "!");
                BotPoi.Current = new BotPoi(_target, PoiType.Kill);
                _currentTarget = _target;
                _target.Target();
                return true;
            }
        }

        private static bool TargetTotems()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (_currentTargetType == "Totems"
                        && StyxWoW.Me.CurrentTarget.Distance <= 10
                        && _currentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(_currentTarget, PoiType.Kill);
                        _target.Target();
                        return true;
                    }
                }
                // Target Totem
                _target = EnemyTotem();
                if (_target == null) return false;
                if (_target.Guid == _currentTarget.Guid) return false;
                Logging.Write(TargetColor, _target.Name + " Spotted!. Switching to " + _target.Name + "!");
                BotPoi.Current = new BotPoi(_target, PoiType.Kill);
                _currentTarget = _target;
                _target.Target();
                return true;
            }
        }

        private static WoWUnit EnemyTotem()
        {
            string[] totemNames = { "Mana Tide Totem", "Earthbind Totem", "Tremor Totem", "Grounding Totem", "Cleansing Totem", "Spirit Link Totem" };

            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from unit in ObjectManager.GetObjectsOfType<WoWUnit>()
                        where unit.CreatedByUnitGuid != StyxWoW.Me.Guid
                        where unit.CreatureType == WoWCreatureType.Totem
                        where unit.Distance < 10
                        where !unit.IsFriendly
                        where totemNames.Contains(unit.Name)
                        where Navigator.NavigationProvider.LookupPathInfo(unit).Navigability == PathNavigability.Navigable//.CanNavigateFully(StyxWoW.Me.Location, unit.Location)
                        select unit).FirstOrDefault();
            }
        }

        private static WoWPlayer ClosestEnemy()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>()
                        where unit.IsAlive
                        where unit.IsPlayer
                        where unit.Distance < 15
                        where !unit.IsFriendly
                        where !unit.IsPet
                        where !unit.HasAura("Spirit of Redemption")
                        where !unit.HasAura("Blessing of Protection")
                        where !unit.HasAura("Divine Shield")
                        where !unit.HasAura("Ice Block")
                        where !unit.HasAura("Cyclone")
                        where Navigator.NavigationProvider.LookupPathInfo(unit).Navigability == PathNavigability.Navigable//.CanNavigateFully(StyxWoW.Me.Location, unit.Location)
                        select unit).OrderBy(u => u.Distance).FirstOrDefault();
            }
        }

        private static WoWPlayer EnemyLowestHealth()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>()
                        orderby unit.HealthPercent
                        where unit.IsAlive
                        where unit.IsPlayer
                        where unit.Distance < 15
                        where !unit.IsFriendly
                        where !unit.IsPet
                        where !unit.HasAura("Spirit of Redemption")
                        where !unit.HasAura("Blessing of Protection")
                        where !unit.HasAura("Divine Shield")
                        where !unit.HasAura("Ice Block")
                        where !unit.HasAura("Cyclone")
                        where Navigator.NavigationProvider.LookupPathInfo(unit).Navigability == PathNavigability.Navigable//.CanNavigateFully(StyxWoW.Me.Location, unit.Location)
                        where unit.HealthPercent < 35
                        select unit).FirstOrDefault();
            }
        }

        private static WoWPlayer EnemyUndergeared()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>()
                        orderby unit.MaxHealth
                        where unit.IsAlive
                        where unit.IsPlayer
                        where unit.Distance < 20
                        where !unit.IsFriendly
                        where !unit.IsPet
                        where !unit.HasAura("Spirit of Redemption")
                        where !unit.HasAura("Blessing of Protection")
                        where !unit.HasAura("Divine Shield")
                        where !unit.HasAura("Ice Block")
                        where !unit.HasAura("Cyclone")
                        where Navigator.NavigationProvider.LookupPathInfo(unit).Navigability == PathNavigability.Navigable//.CanNavigateFully(StyxWoW.Me.Location, unit.Location)
                        select unit).FirstOrDefault();
            }
        }

        private static WoWPlayer EnemyAllianceFlagCarrier()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>()
                        where unit.IsAlive
                        where unit.IsPlayer
                        where unit.Distance < 30
                        where !unit.IsFriendly
                        where !unit.IsPet
                        where unit.HasAura("Alliance Flag")
                        where unit.InLineOfSight
                        where Navigator.NavigationProvider.LookupPathInfo(unit).Navigability == PathNavigability.Navigable//.CanNavigateFully(StyxWoW.Me.Location, unit.Location)
                        select unit).FirstOrDefault();
            }
        }

        private static WoWPlayer EnemyHordeFlagCarrier()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>()
                        where unit.IsAlive
                        where unit.IsPlayer
                        where unit.Distance < 30
                        where !unit.IsFriendly
                        where !unit.IsPet
                        where unit.HasAura("Horde Flag")
                        where unit.InLineOfSight
                        where Navigator.NavigationProvider.LookupPathInfo(unit).Navigability == PathNavigability.Navigable//.CanNavigateFully(StyxWoW.Me.Location, unit.Location)
                        select unit).FirstOrDefault();
            }
        }

        private static WoWPlayer EnemyOrbCarrier()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>()
                        where unit.IsAlive
                        where unit.IsPlayer
                        where unit.Distance < 30
                        where !unit.IsFriendly
                        where !unit.IsPet
                        where unit.HasAura("Orb of Power")
                        where unit.InLineOfSight
                        where Navigator.NavigationProvider.LookupPathInfo(unit).Navigability == PathNavigability.Navigable//.CanNavigateFully(StyxWoW.Me.Location, unit.Location)
                        select unit).FirstOrDefault();
            }
        }

        private static WoWPlayer ValidTarget(string role, int range)
        {
            PlayerSpecCheck();
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>()
                        orderby unit.MaxHealth
                        where unit.IsAlive
                        where unit.IsPlayer
                        where unit.Distance < range
                        where !unit.IsFriendly
                        where !unit.IsPet
                        where !unit.HasAura("Spirit of Redemption")
                        where !unit.HasAura("Blessing of Protection")
                        where !unit.HasAura("Divine Shield")
                        where !unit.HasAura("Iceblock")
                        where !unit.HasAura("Cyclone")
                        where !unit.HasAura("Cloak of Shadows")
                        where !unit.HasAura("Anti-Magic Shell")
                        where Navigator.NavigationProvider.LookupPathInfo(unit).Navigability == PathNavigability.Navigable//.CanNavigateFully(StyxWoW.Me.Location, unit.Location)
                        where CheckHealerList(unit) && role == "Healer" ||
                        CheckCasterList(unit) && role == "Caster" ||
                        CheckMeleeList(unit) && role == "Melee" ||
                        CheckTankList(unit) && role == "Tank"
                        select unit).FirstOrDefault();
            }
        }

        private static readonly List<string> HealerList = new List<string>();
        private static readonly List<string> CasterList = new List<string>();
        private static readonly List<string> MeleeList = new List<string>();
        private static readonly List<string> TankList = new List<string>();
        private static List<string> _playerInfo = new List<string>();
        private static void PlayerSpecCheck()
        {
            try
            {
                string faction = "1";
                if (StyxWoW.Me.IsHorde)
                    faction = "0";
                if (Lua.GetReturnVal<int>("return GetNumBattlefieldScores()", 0) > 0)
                {
                    _playerInfo.Clear();
                    HealerList.Clear();
                    MeleeList.Clear();
                    TankList.Clear();
                    CasterList.Clear();
                    using (StyxWoW.Memory.AcquireFrame())
                    {
                        for (var i = 0; i <= Lua.GetReturnVal<int>("return GetNumBattlefieldScores()", 0); i++)
                        {
                            var p = 0;
                            var playerName = "";

                            _playerInfo = Lua.GetReturnValues("return GetBattlefieldScore(" + i + ")");
                            foreach (string info in _playerInfo)
                            {
                                p++;

                                if (p == 1)
                                {
                                    //Logging.Write(Colors.DarkOrange, "Name: " + playerName);
                                    playerName = info;
                                }
                                if (p == 6)
                                {
                                    if (info == faction)
                                    {
                                        //Logging.Write(Colors.DarkOrange, "Faction: " + playerName);
                                        break;
                                    }
                                }
                                if (p == 9)
                                {
                                    if (info == "Rogue")
                                    {
                                        MeleeList.Add(playerName);
                                        break;
                                    }
                                    if (info == "Warlock" || info == "Mage")
                                    {
                                        CasterList.Add(playerName);
                                        break;
                                    }
                                }
                                if (p != 16) continue;
                                if (info.Contains("Resto") || info.Contains("Disc") || info.Contains("Holy"))
                                {
                                    //Logging.Write(Colors.DarkOrange, "Adding " + playerName + " as a " + info + " healer!");
                                    HealerList.Add(playerName);
                                    break;
                                }
                                if (info.Contains("Prot") || info.Contains("Blood"))
                                {
                                    TankList.Add(playerName);
                                    break;
                                }
                                if (info.Contains("Fury") || info.Contains("Arms") ||
                                    info.Contains("Frost") || info.Contains("Unholy") ||
                                    info.Contains("Retribution") || info.Contains("Feral") || info.Contains("Enhancement"))
                                {
                                    MeleeList.Add(playerName);
                                    break;
                                }
                                CasterList.Add(playerName);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception exg) { Logging.Write(Colors.DarkRed, "" + exg); }
        }

        private static bool CheckHealerList(WoWPlayer unit)
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                if (HealerList.Any(healer => healer.Contains(unit.Name)))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckCasterList(WoWPlayer unit)
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                if (CasterList.Any(caster => caster.Contains(unit.Name)))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckMeleeList(WoWPlayer unit)
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                if (MeleeList.Any(melee => melee.Contains(unit.Name)))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckTankList(WoWPlayer unit)
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                if (TankList.Any(tank => tank.Contains(unit.Name)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
