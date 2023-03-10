using Media_Player.Utils;
using Newtonsoft.Json;

namespace Media_Player.Core
{
    public class LocalStorage
    {
        private static Preferences _prefs = new Preferences("media_player_data");
        private static Preferences _prefs2 = new Preferences("media_player_last_position");

        private const string keyAllPlaylists = "all_playlists";
        private const string keyRecentlyMedia = "recently_media";
        private const string keyLastPlayedPlaylistId = "last_played_playlist_id";
        private const string keyLastPlayedMediaId = "last_played_media_id";
        private const string keyLastPlayedPosition = "last_played_position";
        private const string keyLastPlayedHistory = "last_played_history";

        private const int maxRecentlyMedia = 10;

        public static List<Playlist> getAllPlaylists()
        {
            List<PlaylistAdapter> playlists = _prefs.get<List<PlaylistAdapter>>(keyAllPlaylists) ?? new List<PlaylistAdapter>();
            List<Playlist> originalPlaylists = new List<Playlist>();
            foreach (var playlist in playlists)
            {
                originalPlaylists.Add(playlist.toPlaylist());
            }
            return originalPlaylists;
        }

        public static void saveAllPlaylists()
        {
            List<Playlist> originalPlaylists = GLOBAL.AllPlaylist;
            List<PlaylistAdapter> playlists = new List<PlaylistAdapter>();
            foreach (var origin in originalPlaylists)
            {
                playlists.Add(new PlaylistAdapter(origin));
            }
            _prefs.put(keyAllPlaylists, playlists);
        }

        public static List<Media> getRecentlyMedia()
        {
            List<Media> medias = _prefs.get<List<Media>>(keyRecentlyMedia) ?? new List<Media>();
            return medias;
        }

        public static void addRecentlyMedia(Media media)
        {
            List<Media> medias = getRecentlyMedia();
            medias.RemoveAll(m => m.FilePath == media.FilePath);
            medias.Insert(0, media);
            if (medias.Count > maxRecentlyMedia)
            {
                medias.RemoveAt(medias.Count - 1);
            }
            _prefs.put(keyRecentlyMedia, medias);
        }

        public static void removeRecentlyMedia(Media media)
        {
            List<Media> medias = getRecentlyMedia();
            medias.RemoveAll(m => m.FilePath == media.FilePath);
            _prefs.put(keyRecentlyMedia, medias);
        }

        public static void clearRecentlyMedia()
        {
            _prefs.remove(keyRecentlyMedia);
        }

        public static string? getLastPlayedPlaylistId() => _prefs.get<string>(keyLastPlayedPlaylistId);
        public static string? getLastPlayedMediaId() => _prefs.get<string>(keyLastPlayedMediaId);
        public static int? getLastPlayedPosition() => _prefs2.get<int>(keyLastPlayedPosition);
        public static List<string>? getLastPlayedHistory() => _prefs.get<List<string>>(keyLastPlayedHistory);

        public static void setLastPlayed()
        {
            string? playlistId = GLOBAL.NowPlaylistId;
            if (playlistId == null) return;

            string? mediaId = GLOBAL.Controller.NowId;
            if (mediaId == null) return;

            List<string> history = GLOBAL.Controller.History;

            _prefs.put(keyLastPlayedPlaylistId, playlistId);
            _prefs.put(keyLastPlayedMediaId, mediaId);
            _prefs.put(keyLastPlayedHistory, history);
        }

        public static void setLastPlayedPosition()
        {
            int? position = GLOBAL.Controller.Position;
            if (position.HasValue)
                _prefs2.put(keyLastPlayedPosition, position);
        }

        public static void clearLastPlayed()
        {
            _prefs.remove(keyLastPlayedPlaylistId);
            _prefs.remove(keyLastPlayedMediaId);
            _prefs.remove(keyLastPlayedHistory);
            _prefs2.remove(keyLastPlayedPosition);
        }

        private class PlaylistAdapter
        {
            public string Id { get; }
            public string Name { get; set; }

            public List<Media> ListMedia = new List<Media>();

            [JsonConstructor]
            public PlaylistAdapter(string id, string name, List<Media> listMedia)
            {
                Id = id;
                Name = name;
                ListMedia = listMedia;
            }

            public PlaylistAdapter(Playlist origin)
            {
                Id = origin.Id;
                Name = origin.Name;
                ListMedia = origin.ListMedia.ToList();
            }

            public Playlist toPlaylist() => new Playlist(Id, Name, ListMedia);
        }
    }
}
