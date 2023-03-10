using System.Windows.Media;

namespace Media_Player.Utils
{
    public class Utilities
    {
        public const string FOCUS_COLOR = "#5F9DF7";
        public const string UNFOCUS_COLOR = "#27273A";

        public static SolidColorBrush ToSolidColorBrush(string hex_code)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(hex_code);
        }

        public static SolidColorBrush FocusColor = ToSolidColorBrush(FOCUS_COLOR);
        public static SolidColorBrush UnfocusColor = ToSolidColorBrush(UNFOCUS_COLOR);
    }
}
