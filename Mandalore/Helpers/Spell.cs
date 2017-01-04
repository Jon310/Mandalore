using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using JetBrains.Annotations;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace Mandalore.Helpers
{
    [UsedImplicitly]
    abstract class Spell : Mandalore
    {

        #region CoCast Wrappers

        public static async Task<bool> CoCast(int spell)
        {
            return await CoCast(spell, Me.CurrentTarget, true, false);
        }

        public static async Task<bool> CoCast(int spell, WoWUnit unit)
        {
            return await CoCast(spell, unit, true, false);
        }

        public static async Task<bool> CoCast(int spell, bool reqs)
        {
            return await CoCast(spell, Me.CurrentTarget, reqs, false);
        }

        public static async Task<bool> CoCast(int spell, WoWUnit unit, bool reqs)
        {
            return await CoCast(spell, unit, reqs, false);
        }

        #endregion

        #region PetCast

        public static async Task<bool> PetCast(string spell, WoWUnit unit, bool reqs)
        {
            var petspell = new List<WoWPetSpell>().FirstOrDefault(p => p.ToString() == spell);

            if (petspell == null)
                return false;

            if ((!reqs || !petspell.Spell.CanCast || unit == null))
                return false;
            
            Lua.DoString("CastPetAction({0})", petspell.ActionBarIndex + 1);


            if (!await Coroutine.Wait(1000, () => StyxWoW.Me.CurrentPendingCursorSpell != null))
            {
                Logging.Write(Colors.DarkRed, "Cursor Spell Didn't happen");
                return false;
            }

            var onLocation = unit.Location;
            SpellManager.ClickRemoteLocation(onLocation);
            Log.WritetoFile(LogLevel.Diagnostic, $"Casting {spell}");
            await CommonCoroutines.SleepForLagDuration();
            return true;
        }

        public static async Task<bool> PetCast(string spell, Vector3 location, bool reqs)
        {
            var petspell = new List<WoWPetSpell>().FirstOrDefault(p => p.ToString() == spell);

            if (petspell == null)
                return false;

            if (!reqs || !petspell.Spell.CanCast)
                return false;

            Lua.DoString("CastPetAction({0})", petspell.ActionBarIndex + 1);

            if (!await Coroutine.Wait(1000, () => StyxWoW.Me.CurrentPendingCursorSpell != null))
            {
                Logging.Write(Colors.DarkRed, "Cursor Spell Didn't happen");
                return false;
            }

            SpellManager.ClickRemoteLocation(location);
            Log.WritetoFile(LogLevel.Diagnostic, $"Casting {spell}");
            await CommonCoroutines.SleepForLagDuration();
            return true;
        }

        #endregion

        #region CoCast

        public static async Task<bool> CoCast(int spell, WoWUnit unit, bool reqs, bool cancel)
        {
            var sp = WoWSpell.FromId(spell);
            var sname = sp != null ? sp.Name : "#" + spell;

            if (unit == null || !reqs || !SpellManager.CanCast(spell, unit, true))
                return false;

            if (!SpellManager.Cast(spell, unit))
                return false;

            //if (!await Coroutine.Wait(GetSpellCastTime(sname), () => cancel) && GetSpellCastTime(sname).TotalSeconds > 0)
            //{
            //    SpellManager.StopCasting();
            //    Log.WriteLog("Canceling " + sname + ".");
            //    return false;
            //}

            Log.WriteLog(LogLevel.Normal, $"Casting {sname}", Colors.Peru);

            await CommonCoroutines.SleepForLagDuration();
            return true;
        }

        #endregion

        #region CastSpell

        public static async Task<bool> CastSpell(string spell, WoWUnit onunit, bool reqs, string reason = "", bool ignoregcd = false)
        {
            if (!reqs)
            {
                return false;
            }

            if (!SpellManager.CanCast(spell))
            {
                return false;
            }
            //if (!await Movement.FaceTarget(onunit))
            //    return false;
            if (SpellManager.Cast(spell, onunit))
            {
                Log.WriteLog(LogLevel.Normal, $"Casting {spell}", Colors.Peru);
                await CommonCoroutines.SleepForLagDuration();
                return true;
            }
            return false;
        }
        #endregion

        #region CastOnGround

        public static async Task<bool> CastOnGround(int spell, WoWUnit unit, bool reqs)
        {
            var sp = WoWSpell.FromId(spell);
            var sname = sp != null ? sp.Name : "#" + spell;

            if (!reqs || !SpellManager.CanCast(spell) || unit == null)
                return false;

            var onLocation = unit.Location;

            if (!SpellManager.Cast(spell))
                return false;

            await CommonCoroutines.SleepForLagDuration();

            if (!await Coroutine.Wait(1000, () => StyxWoW.Me.CurrentPendingCursorSpell != null))
            {
                Logging.Write(Colors.DarkRed, "Cursor Spell Didn't happen");
                return false;
            }

            SpellManager.ClickRemoteLocation(onLocation);
            Log.WriteLog(LogLevel.Normal, $"Casting {sname}", Colors.Peru);
            Log.WritetoFile(LogLevel.Diagnostic, $"Casting {sname}");
            await CommonCoroutines.SleepForLagDuration();
            return true;
        }

        #endregion

        #region GetSpellCastTime(string s)

        public static TimeSpan GetSpellCastTime(string s)
        {
            SpellFindResults sfr;
            if (SpellManager.FindSpell(s, out sfr))
                return TimeSpan.FromMilliseconds((sfr.Override ?? sfr.Original).CastTime);
            return TimeSpan.Zero;
        }

        #endregion

        #region GetCharges

        public static int GetCharges(string name)
        {
            SpellFindResults sfr;
            if (!SpellManager.FindSpell(name, out sfr)) return 0;
            var spell = sfr.Override ?? sfr.Original;
            return GetCharges(spell);
        }

        public static int GetCharges(int name)
        {
            SpellFindResults sfr;
            if (!SpellManager.FindSpell(name, out sfr)) return 0;
            var spell = sfr.Override ?? sfr.Original;
            return GetCharges(spell);
        }

        private static int GetCharges(WoWSpell spell)
        {
            var charges = Lua.GetReturnVal<int>("return GetSpellCharges(" + spell.Id + ")", 0);
            return charges;
        }

        #endregion

        #region GetCooldownLeft

        public static TimeSpan GetCooldownLeft(string spell)
        {
            SpellFindResults results;
            if (!SpellManager.FindSpell(spell, out results)) return TimeSpan.MaxValue;
            return results.Override?.CooldownTimeLeft ?? results.Original.CooldownTimeLeft;
        }

        public static TimeSpan GetCooldownLeft(int spell)
        {
            SpellFindResults results;
            if (!SpellManager.FindSpell(spell, out results)) return TimeSpan.MaxValue;
            return results.Override?.CooldownTimeLeft ?? results.Original.CooldownTimeLeft;
        }

        #endregion


    }
}
