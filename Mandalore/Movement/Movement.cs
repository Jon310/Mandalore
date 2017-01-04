using System;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Styx;
using Styx.CommonBot.Coroutines;
using Styx.CommonBot.Routines;
using Styx.Helpers;
using Styx.WoWInternals;

namespace Mandalore.Movement
{
    [UsedImplicitly]
    abstract class Movement : Mandalore
    {
        public static async Task<bool> PulseMovemenTask()
        {
            if (ValidChecks())
                return true;

            if (!IsAllowed(CapabilityFlags.Movement))
                return true;

            if (Me.CurrentTarget.IsFlying)
            {
                Me.SetFacing(Me.CurrentTarget);
                await CommonCoroutines.MoveTo(Me.CurrentTarget.Location);
            }
                

            if (!Me.CurrentTarget.InLineOfSight || Me.CurrentTarget.Distance > 10 || !Me.CurrentTarget.IsPlayer || !Me.CurrentTarget.IsPetBattleCritter)
            {
                if (IsRanged())
                {
                    if (!Me.IsSafelyFacing(Me.CurrentTarget))
                        Me.SetFacing(Me.CurrentTarget);

                    if (Me.CurrentTarget.Distance < 40)
                    {
                        await CommonCoroutines.StopMoving();
                    }

                    if (Me.CurrentTarget.Distance > 40)
                        await CommonCoroutines.MoveTo(Me.CurrentTarget.Location);

                    return true;
                }
                else
                {
                    var meleeRangeOf = Me.CurrentTarget.GetMeleeRangeOf(Me);

                    if (Vector3.DistanceSquared(Me.Location, Me.CurrentTarget.Location) < (meleeRangeOf * meleeRangeOf) + 3)
                    {
                        if (!Me.IsSafelyFacing(Me.CurrentTarget))
                            Me.SetFacing(Me.CurrentTarget);
                        if (Me.CurrentTarget.IsWithinMeleeRange)
                            await CommonCoroutines.StopMoving();
                    }

                    if (!Me.CurrentTarget.IsWithinMeleeRange)
                        await CommonCoroutines.MoveTo(Me.CurrentTarget.Location);
                    //Navigator.MoveTo(Me.CurrentTarget.Location));
                    return true;
                }
            }

            if (!Me.IsFacing(Me.CurrentTarget) && IsAllowed(CapabilityFlags.Facing))
                Me.SetFacing(Me.CurrentTarget);

            await CheckMoving();
            await CheckStop();
            CheckStrafe();

            return false;
        }

        public static bool IsRanged()
        {
            if (Me.Class == WoWClass.DemonHunter)
                return false;
            if (Me.Class == WoWClass.DeathKnight)
                return false;
            if (Me.Class == WoWClass.Hunter && Me.Specialization == WoWSpec.HunterSurvival)
                return false;
            if (Me.Class == WoWClass.Warrior)
                return false;
            if (Me.Class == WoWClass.Rogue)
                return false;
            if (Me.Class == WoWClass.Paladin && Me.Specialization != WoWSpec.PaladinHoly)
                return false;
            if (Me.Class == WoWClass.Druid && (Me.Specialization == WoWSpec.DruidFeral || Me.Specialization == WoWSpec.DruidGuardian))
                return false;
            if (Me.Class == WoWClass.Monk && (Me.Specialization != WoWSpec.MonkMistweaver))
                return false;
            if (Me.Class == WoWClass.Shaman && Me.Specialization == WoWSpec.ShamanEnhancement)
                return false;

            return true;
        }


        private static bool ValidChecks() => !StyxWoW.IsInGame
                                             || !Me.IsValid
                                             || Me.CurrentTarget == null
                                             || !Me.GotTarget
                                             || Me.Mounted
                                             || Me.IsDead
                                             || Me.CurrentTarget.IsDead
                                             || Me.CurrentTarget.IsFriendly
                                             || !Me.CurrentTarget.Attackable;

        private static async Task<bool> CheckMoving()
        {
            if (Me.CurrentTarget.Distance >= 2 && Me.CurrentTarget.IsMoving && !Me.MovementInfo.MovingForward)
            {
                WoWMovement.Move(WoWMovement.MovementDirection.Forward);
                //Navigator.MoveTo(StyxWoW.Me.CurrentTarget.Location);
                return true;
            }


            if (Me.CurrentTarget.Distance < 2 && Me.CurrentTarget.IsMoving && Me.MovementInfo.MovingForward)
            {
                WoWMovement.MoveStop(WoWMovement.MovementDirection.Forward);
                //StopMoving.InMeleeRangeOfUnit(StyxWoW.Me.CurrentTarget);
                return true;
            }

            if ((Me.MovementInfo.MovingStrafeRight || Me.MovementInfo.MovingStrafeLeft ||
                 Me.MovementInfo.MovingForward && Me.CurrentTarget.IsSafelyBehind(Me)) && Me.CurrentTarget.Distance > 5)
                await CommonCoroutines.StopMoving();
                //StopMoving.Now();//StopMovement(false, false, true, true);

            return false;
        }

        private static async Task<bool> CheckStop()
        {
            if (Me.CurrentTarget.IsMoving) return false;
            const float distance = 3.2f;

            if (Me.CurrentTarget.Distance >= distance && StyxWoW.Me.IsMoving == false)//&& !Me.MovementInfo.MovingForward)
            {
                WoWMovement.ClickToMove(Me.CurrentTarget.Location);
                //WoWMovement.Move(WoWMovement.MovementDirection.Forward, new TimeSpan(99, 99, 99));
                return true;
            }

            // To stop from spinning
            if (Me.CurrentTarget.Distance < 2 && Me.IsMoving)
            {
                //await CommonCoroutines.StopMoving(); //Causes Stutter stepping
                WoWMovement.MoveStop();
            }

            return false;
        }

        private const int Cone = 40;
        private static void CheckStrafe()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                // Test
                if (Me.Stunned) return;

                // Cancel all strafes - distance
                if (Me.MovementInfo.MovingStrafeRight && Me.CurrentTarget.Distance >= 2.5)
                {
                    WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeRight);
                    return;
                }

                if (Me.MovementInfo.MovingStrafeLeft && Me.CurrentTarget.Distance >= 2.5)
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
                if (!Me.CurrentTarget.IsWithinMeleeRange) return;


                // 180 > strafe right
                if (GetDegree >= 180 && GetDegree <= (360 - Cone) && !Me.MovementInfo.MovingStrafeRight)
                {
                    WoWMovement.Move(WoWMovement.MovementDirection.StrafeRight, new TimeSpan(0, 0, 1));
                    return;
                }

                // 180 < strafe left
                if (GetDegree <= 180 && GetDegree >= Cone && !Me.MovementInfo.MovingStrafeLeft)
                {
                    WoWMovement.Move(WoWMovement.MovementDirection.StrafeLeft, new TimeSpan(0, 0, 1));
                }
            }
        }

        private static double GetDegree
        {
            get
            {
                var d = System.Math.Atan2((Me.CurrentTarget.Y - Me.Y), (Me.CurrentTarget.X - Me.X));

                var r = d - Me.CurrentTarget.Rotation; 	  // substracting object rotation from absolute rotation
                if (r < 0)
                    r += (System.Math.PI * 2);

                return WoWMathHelper.RadiansToDegrees((float)r);
            }
        }
    }
}
