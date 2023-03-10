namespace Media_Player.Utils
{
    public static class TimeUtils
    {
        public static string FormatTime(int seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            if (seconds >= 3600)
                return timeSpan.ToString("h':'mm':'ss");
            else return timeSpan.ToString("m':'ss");
        }
    }
}
