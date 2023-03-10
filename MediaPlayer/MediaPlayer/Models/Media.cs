using System.IO;
using Media_Player.Utils;

namespace Media_Player
{
    public class Media
    {
        public string Id { get; }
        public string Name { get; }
        public int Duration { get; }
        public string FilePath { get; }

        public Media(string filePath, string id)
        {
            Id = id;
            FilePath = filePath;
            Duration = FileUtils.GetMediaDuration(filePath);
            Name = Path.GetFileNameWithoutExtension(filePath);
        }

        public bool isMusic => FileUtils.IsAudio(FilePath);
        public bool isVideo => FileUtils.IsVideo(FilePath);

        public string DurationString => TimeUtils.FormatTime(Duration);



    }
}
