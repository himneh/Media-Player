using Media_Player.Utils;
using System.ComponentModel;

namespace Media_Player
{
    public class Playlist
    {
        // == ATTRIBUTES ======================================

        public string Id { get; }
        public string Name { get; set; }

        public BindingList<Media> ListMedia;
        
        public Playlist()
        {
            Id = DateTime.Now.ToString("yy_MM_dd_HH_mm_s_f");
            Name = "Unnamed playlist";
            ListMedia = new BindingList<Media>();
        }

        public Playlist(string id, string name, List<Media> medias)
        {
            Id = id;
            Name = name;
            ListMedia = new BindingList<Media>(medias);
        }


        // == GETTER ======================================

        public int TotalDuration
        {
            get
            {
                int total = 0;
                foreach (var media in ListMedia)
                {
                    total += media.Duration;
                }
                return total;
            }
        }

        public string TotalDurationString => TimeUtils.FormatTime(TotalDuration);

        public bool IsEmpty => ListMedia.Count == 0;
        public bool IsNotEmpty => !IsEmpty;


        // == METHOD ======================================

        public bool AddMedia(Media media)
        {
            // Không thể trùng Id
            if (getIndexById(media.Id) != -1) return false;
            ListMedia.Add(media);
            return true;
        }

        public bool RemoveMedia(Media media)
        {
            return ListMedia.Remove(media);
        }

        public int getIndexById(string mediaId)
        {
            for (int i = 0; i < ListMedia.Count; i++)
            {
                if (ListMedia[i].Id == mediaId) return i;
            }
            return -1;
        }
    }
}
