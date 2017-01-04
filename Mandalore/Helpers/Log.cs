using System;
using System.Windows.Media;
using Styx;
using Styx.Common;

namespace Mandalore.Helpers
{
    class Log
    {
        public static void WriteLog(string text)
        {
            Logging.Write(text);
        }
        public static void WriteLog(string text, Color Color)
        {
            Logging.Write(Color, text);
        }
        public static void WriteLog(LogLevel level, string text)
        {
            if (Styx.Helpers.GlobalSettings.Instance.LogLevel >= level)
                Logging.Write(text);
        }
        public static void WriteLog(LogLevel level, string text, Color Color)
        {
            if (Styx.Helpers.GlobalSettings.Instance.LogLevel >= level)
                Logging.Write(Color, text);
        }
        public static void WriteQuiet(string text)
        {
            Logging.WriteQuiet(text);
        }
        public static void WriteQuiet(string text, Color Color)
        {
            Logging.WriteQuiet(Color, text);
        }
        public static void WriteQuiet(LogLevel level, string text)
        {
            if (Styx.Helpers.GlobalSettings.Instance.LogLevel >= level)
                Logging.WriteQuiet(text);
        }
        public static void WriteQuite(LogLevel level, string text, Color Color)
        {
            if (Styx.Helpers.GlobalSettings.Instance.LogLevel >= level)
                Logging.WriteQuiet(Color, text);
        }
        public static void WritetoFile(string text)
        {
            Logging.WriteToFileSync(LogLevel.Normal, text);
        }
        public static void WritetoFile(LogLevel level, string text)
        {
            if (Styx.Helpers.GlobalSettings.Instance.LogLevel >= level)
                Logging.WriteToFileSync(level, text);
        }

        public static void Toast(string template, params object[] args)
        {
            string msg = string.Format(template, args);
            
            StyxWoW.Overlay.AddToast(() => msg,
                TimeSpan.FromSeconds(1.5),
                Colors.White,
                Colors.Black,
                new FontFamily("Segoe UI"));
        }

        public static void ToastEnabled(string template, params object[] args)
        {
            Toast(template, Colors.SeaGreen, Colors.LightSlateGray, args);
        }

        public static void ToastDisabled(string template, params object[] args)
        {
            Toast(template, Colors.Tomato, Colors.LightSlateGray, args);
        }

        public static void Toast(string template, Color color1, Color color2, params object[] args)
        {
            string msg = string.Format(template, args);

            StyxWoW.Overlay.AddToast(() => msg,
                TimeSpan.FromSeconds(1.5),
                color1,
                color2,
                new FontFamily("Segoe UI"));
        }

    }
}
