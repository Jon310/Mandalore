using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using Mandalore.Helpers;
using Styx;
using Styx.Common;
using Styx.Helpers;
using Styx.Pathing;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace Mandalore.Movement
{
    static class PvPMovement
    {
        private static readonly WoWPlayer Me = StyxWoW.Me;
        private static WoWUnit _target;
        private const int Cone = 40;

        internal static void PulseMovement()
        {
            try
            {

                // Experimenting with Facing
                //if (Me.CurrentTarget == null) WoWMovement.StopFace();


                // Should we move?
                if (StyxWoW.IsInGame == false) return;
                if (Me.IsValid == false) return;
                if (Me.CurrentTarget == null) return;
                if (Me.GotTarget == false) return;
                if (Me.Mounted) return;
                if (Me.IsDead) return;
                if (Me.CurrentTarget.IsPlayer == false) return;

                // Target Check.
                _target = Me.CurrentTarget;
                if (_target.IsDead) return;
                if (_target.IsFriendly) return;
                if (!_target.Attackable) return;

                if (!_target.IsPlayer)
                {
                    Navigator.MoveTo(_target.Location);
                    return;
                }

                // Use default navigator if too far away.
                if (_target.Distance > 10)
                {
                    Navigator.MoveTo(_target.Location);
                    return;
                }

                // Use Default Navigator if not in line of sight.
                if (!_target.InLineOfSight)
                {
                    Navigator.MoveTo(_target.Location);
                    return;
                }

                // Move!
                CheckFace();
                if (CheckMoving()) return;
                if (CheckStop()) return;
                CheckStrafe();
            }
            catch (Exception ex)
            {
                Logging.Write(Colors.DarkRed, "Error in Movement Pulse: " + ex);
            }
        }

        private static void CheckFace()
        {
            if (!WoWMovement.IsFacing)
            {
                WoWMovement.Face(_target.Guid);
            }
        }

        private static bool CheckMoving()
        {
            if (_target.Distance >= 2 && _target.IsMoving && !Me.MovementInfo.MovingForward)
            {
                WoWMovement.Move(WoWMovement.MovementDirection.Forward);
                //Navigator.MoveTo(StyxWoW.Me.CurrentTarget.Location);
                return true;
            }


            if (_target.Distance < 2 && _target.IsMoving && Me.MovementInfo.MovingForward)
            {
                WoWMovement.MoveStop(WoWMovement.MovementDirection.Forward);
                //StopMoving.InMeleeRangeOfUnit(StyxWoW.Me.CurrentTarget);
                return true;
            }

            if ((Me.MovementInfo.MovingStrafeRight || Me.MovementInfo.MovingStrafeLeft || Me.MovementInfo.MovingForward && Me.CurrentTarget.IsSafelyBehind(Me)) && Me.CurrentTarget.Distance > 5)
                StopMoving.Now();//StopMovement(false, false, true, true);

            return false;
        }

        private static bool CheckStop()
        {
            if (_target.IsMoving) return false;
            const float distance = 3.2f;

            if (_target.Distance >= distance && StyxWoW.Me.IsMoving == false)//&& !Me.MovementInfo.MovingForward)
            {
                WoWMovement.ClickToMove(_target.Location);
                //WoWMovement.Move(WoWMovement.MovementDirection.Forward, new TimeSpan(99, 99, 99));
                return true;
            }

            // To stop from spinning
            if (_target.Distance < 2 && Me.IsMoving)
            {
                WoWMovement.MoveStop();
            }

            return false;
        }

        private static void CheckStrafe()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                // Test
                if (Me.Stunned) return;

                // Cancel all strafes - distance
                if (Me.MovementInfo.MovingStrafeRight && _target.Distance >= 2.5)
                {
                    WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeRight);
                    return;
                }

                if (Me.MovementInfo.MovingStrafeLeft && _target.Distance >= 2.5)
                {
                    WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeLeft);
                    return;
                }

                // Cancel all strafes - Angle out of range
                if (Me.MovementInfo.MovingStrafeRight && GetDegree <= 180 && GetDegree >= Cone)
                {
                    WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeRight);
                    return;
                }
                if (Me.MovementInfo.MovingStrafeLeft && GetDegree >= 180 && GetDegree <= (360 - Cone))
                {
                    WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeLeft);
                    return;
                }

                // Dont strafe if we are not close enough
                if (!_target.IsWithinMeleeRange) return;


                // 180 > strafe right
                if (GetDegree >= 180 && GetDegree <= (360 - Cone) && !Me.MovementInfo.MovingStrafeRight)
                {
                    WoWMovement.Move(WoWMovement.MovementDirection.StrafeRight, new TimeSpan(0, 0, 1));
                    return;
                }

                // 180 < strafe left
                if (!(GetDegree <= 180) || !(GetDegree >= Cone) || Me.MovementInfo.MovingStrafeLeft) return;
                WoWMovement.Move(WoWMovement.MovementDirection.StrafeLeft, new TimeSpan(0, 0, 1));
            }
        }

        private static double GetDegree
        {
            get
            {
                var d = System.Math.Atan2((_target.Y - Me.Y), (_target.X - Me.X));

                var r = d - _target.Rotation; 	  // substracting object rotation from absolute rotation
                if (r < 0)
                    r += (System.Math.PI * 2);

                return WoWMathHelper.RadiansToDegrees((float)r);
            }
        }

        public static bool InMoveToMeleeStopRange(WoWUnit unit)
        {
            if (unit == null || !unit.IsValid)
                return true;

            if (unit.IsPlayer)
            {
                if (unit.DistanceSqr < (2 * 2))
                    return true;

            }
            else
            {
                var preferredDistance = Me.GetMeleeRangeOf(unit) - 1f;
                if (unit.Distance <= preferredDistance)
                    return true;
            }

            return !unit.IsMoving && unit.IsWithinMeleeRange;
        }



        public static bool InLineOfSpellSight(WoWUnit unit)
        {
            if (unit.InLineOfSpellSight)
            {

                if (unit.IsWithinMeleeRange)
                {
                    Log.WriteLog(LogLevel.Diagnostic, "InLineOfSpellSight: last LoS error < 1 sec but in melee range, pretending we are in LoS");
                    //Logger.WriteDebug( Color.White, "InLineOfSpellSight: last LoS error < 1 sec but in melee range, pretending we are in LoS");
                    return true;
                }

                Log.WriteLog(LogLevel.Diagnostic, "InLineOfSpellSight: last LoS error < 1 sec, pretending still not in LoS");
                //Logger.WriteDebug( Color.White, "InLineOfSpellSight: last LoS error < 1 sec, pretending still not in LoS");
                return false;

            }

            return false;
        }

    }

    public delegate bool SimpleBooleanDelegate(object context);
    public static class StopMoving
    {
        private static StopType Type { get; set; }
        private static WoWPoint Point { get; set; }
        private static WoWUnit Unit { get; set; }
        private static double Range { get; set; }
        private static SimpleBooleanDelegate StopNow { get; set; }

        private enum StopType
        {
            None = 0,
            AsSoonAsPossible,
            Location,
            RangeOfLocation,
            RangeOfUnit,
            MeleeRangeOfUnit,
            LosOfUnit
        }

        static StopMoving()
        {
            Clear();
        }

        private static void Clear()
        {
            Set(StopType.None, null, WoWPoint.Empty, 0, stop => false, null);
        }

        public static void Pulse()
        {
            if (Type == StopType.None)
                return;

            bool stopMovingNow;
            try
            {
                stopMovingNow = StopNow(null);
            }
            catch
            {
                stopMovingNow = true;
            }

            if (stopMovingNow)
            {
                if (!StyxWoW.Me.IsMoving)
                    Log.WriteLog(LogLevel.Diagnostic, "StopMoving: character already stopped, clearing stop " + Type + " request");
                //Logger.WriteDebug(Color.White, "StopMoving: character already stopped, clearing stop {0} request", Type);
                else
                {
                    Navigator.PlayerMover.MoveStop();

                    string line = string.Format("StopMoving: {0}", Type);
                    if (Type == StopType.Location)
                        line += string.Format(", within {0:F1} yds of {1}", StyxWoW.Me.Location.Distance(Point), Point);
                    else if (Type == StopType.RangeOfLocation)
                        line += string.Format(", within {0:F1} yds of {1} @ {2:F1} yds", Range, Point, StyxWoW.Me.Location.Distance(Point));
                    else if (Unit == null || !Unit.IsValid)
                        line += ", unit == null";
                    else if (Type == StopType.LosOfUnit)
                        line += string.Format(", have LoSS of {0} @ {1:F1} yds", Unit.Name, Unit.Distance);
                    else if (Type == StopType.MeleeRangeOfUnit)
                        line += string.Format(", within melee range of {0} @ {1:F1} yds", Unit.Name, Unit.Distance);
                    else if (Type == StopType.RangeOfUnit)
                        line += string.Format(", within {0:F1} yds of {1} @ {2:F1} yds", Range, Unit.Name, Unit.Distance);

                    //Logging.WriteDiagnostic(line);
                    //Logger.WriteDebug(Color.White, line);
                }

                Clear();
            }
        }

        private static void Set(StopType type, WoWUnit unit, WoWPoint pt, double range, SimpleBooleanDelegate stop, SimpleBooleanDelegate and)
        {
            //if (MovementManager.IsMovementDisabled)
            //    return;

            Type = type;
            Unit = unit;
            Point = pt;
            Range = range;

            if (and == null)
                and = ret => true;

            StopNow = ret => stop(ret) && and(ret);
        }

        public static void AtLocation(WoWPoint pt, SimpleBooleanDelegate and = null)
        {
            Set(StopType.Location, null, pt, 0, at => StyxWoW.Me.Location.Distance(pt) <= 1, and);
        }

        public static void InRangeOfLocation(WoWPoint pt, double range, SimpleBooleanDelegate and = null)
        {
            Set(StopType.RangeOfLocation, null, pt, range, at => StyxWoW.Me.Location.Distance(pt) <= range, and);
        }

        public static void InRangeOfUnit(WoWUnit unit, double range, SimpleBooleanDelegate and = null)
        {
            Set(StopType.RangeOfUnit, unit, WoWPoint.Empty, range, at => Unit == null || !Unit.IsValid || Unit.Distance <= range, and);
        }

        public static void InMeleeRangeOfUnit(WoWUnit unit, SimpleBooleanDelegate and = null)
        {
            Set(StopType.RangeOfUnit, unit, WoWPoint.Empty, 0, at => Unit == null || !Unit.IsValid || PvPMovement.InMoveToMeleeStopRange(Unit), and);
        }

        public static void InLosOfUnit(WoWUnit unit, SimpleBooleanDelegate and = null)
        {
            Set(StopType.LosOfUnit, unit, WoWPoint.Empty, 0, at => Unit == null || !Unit.IsValid || PvPMovement.InLineOfSpellSight(Unit), and);
        }

        public static void Now()
        {
            Clear();
            if (StyxWoW.Me.IsMoving)
            {
                //Logging.WriteDiagnostic("StopMoving.Now: character already stopped, clearing stop {0} request", Type);
                //Logger.WriteDebug(Color.White, "StopMoving.Now: character already stopped, clearing stop {0} request", Type);
                Navigator.PlayerMover.MoveStop();
            }
        }

        public static void AsSoonAsPossible(SimpleBooleanDelegate and = null)
        {
            Set(StopType.AsSoonAsPossible, null, WoWPoint.Empty, 0, at => true, and);
        }
    }


}

