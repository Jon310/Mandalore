using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Styx;

namespace Mandalore.Helpers
{
    class KeyboardPolling
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();


        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);


        public static bool IsKeyDown(Keys key, bool gameWindowFocused = true)
        {
            if (gameWindowFocused && GetForegroundWindow() != StyxWoW.Memory.Process.MainWindowHandle)
                return false;


            return (GetAsyncKeyState(key) & 0x8000) != 0;
        }
    }
}
