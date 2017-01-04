using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.CommonBot.POI;
using Styx.WoWInternals.WoWObjects;

namespace Mandalore.Helpers
{
    static class PullMore
    {
        private static LocalPlayer Me = StyxWoW.Me;
        private static WoWUnit Target = Me.CurrentTarget;
        public static async Task<bool> PullMoreMobs()
        {
            if (Me.HealthPercent < 70)
                return false;

            if (Units.EnemyUnitsSub40.Count(u => u.IsTargetingMeOrPet) >= 4)
                return false;

            if (BotPoi.Current.Type != PoiType.Kill)
                return false;

            var pullTarget = Units.EnemyUnitsSub40.Where(u =>
                        !u.IsPet && !u.IsTagged && !u.IsPetBattleCritter && 
                        !u.IsPlayer && !u.Combat && !u.GotTarget && u.Attackable && 
                        u.CanSelect && !u.IsDead && !u.IsFriendly && !u.IsNonCombatPet && 
                        u.Guid != Me.CurrentTarget.Guid && 
                        !Blacklist.Contains(u, BlacklistFlags.Combat | BlacklistFlags.Pull) && u.Distance <= 45)
                        .OrderBy(u => u.Distance).FirstOrDefault();

            if (pullTarget != null)
            {
                Log.WriteLog(LogLevel.Normal, $"Pulling More: Mob - {pullTarget.Name}", Colors.Peru);
                BotPoi.Current = new BotPoi(pullTarget, PoiType.Kill, NavType.Run);
                pullTarget.Target();
                await CommonCoroutines.SleepForLagDuration();
            }

            return false;
        } 


    }
}
