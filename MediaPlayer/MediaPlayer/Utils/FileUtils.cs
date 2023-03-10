using System.IO;
using System.Windows.Media;

namespace Media_Player.Utils
{
    public static class FileUtils
    {
        public static int GetMediaDuration(string filePath)
        {
            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(filePath));
            while (!mediaPlayer.NaturalDuration.HasTimeSpan) ;
            int duration = (int)mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            return duration;
        }

        public static string MEDIA_FILTER = "(mp3, wav, mp4, wmv, mpg, avi, flv)| *.mp3; *.wav; *.mp4; *.avi; *.flv; *.wmv; *.mpg | all files | *.* ";

        public static string[] VIDEO_EXTENSIONS =
        {
            ".FLV",".AVI",".WMV",".MP4",".MPG",".MPEG",".M4V"
        };

        public static string[] AUDIO_EXTENSIONS =
        {
            ".MP3",".WAV",".WAVE",".WMA"
        };

        public static bool IsAudio(string filePath)
        {
            return -1 != Array.IndexOf(AUDIO_EXTENSIONS, Path.GetExtension(filePath).ToUpperInvariant());
        }

        public static bool IsVideo(string filePath)
        {
            return -1 != Array.IndexOf(VIDEO_EXTENSIONS, Path.GetExtension(filePath).ToUpperInvariant());
        }

        public static bool IsMedia(string filePath)
        {
            return IsAudio(filePath) || IsVideo(filePath);
        }
    }
}
