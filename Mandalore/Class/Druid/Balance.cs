using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mandalore.Helpers;
using Styx;
using Styx.CommonBot.Coroutines;
using S = Mandalore.Helpers.SpellList;

namespace Mandalore.Class.Druid
{
    class Balance : Mandalore
    {
        #region Overrides
        public override WoWClass Class => Me.Specialization == WoWSpec.DruidBalance ? WoWClass.Druid : WoWClass.None;
        #endregion
        protected async override Task<bool> CreateCombat()
        {
            if (!Me.Combat || Me.Mounted || !Me.GotTarget || !Me.CurrentTarget.IsAlive) return true;

            //if (await PullMore.PullMoreMobs())
            //    return false;


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
                
            }


            await CommonCoroutines.MoveTo(Me.CurrentTarget.Location);

            return false;
        }

        protected async override Task<bool> CreateBuffs()
        {
            return false;
        }

        protected async override Task<bool> CreateRest()
        {
            return await Helpers.Rest.CreateRestHp();
        }

        protected async override Task<bool> CreateHeal()
        {
            return false;
        }
    }
}
