using Media_Player.Utils;
using ControlButton = System.Windows.Controls;

namespace Media_Player.Core
{
    public static class GLOBAL
    {
        const int NOTFOUND_INDEX = -1;

        // == ATTRIBUTES ======================================

        public static PlayerController Controller = new PlayerController();

        public static List<Playlist> AllPlaylist = new List<Playlist>();

        public static string? CurrPlaylistId;

        public static string? NowPlaylistId;

        public static bool IsPlayingRecently = false;

        public static ControlButton.Button? RecentlyPlayedButton { get; set; }
        public static ControlButton.Button? AboutUsButton { get; set; }
        public static List<ControlButton.Button> PlayListButton { get; set; } = new List<ControlButton.Button>();

        // == GETTER ======================================

        public static int CurrPlaylistIndex => getPlaylistIndexById(CurrPlaylistId);
        public static int NowPlaylistIndex => getPlaylistIndexById(NowPlaylistId);
        public static Playlist? CurrPlaylist => getPlaylistById(CurrPlaylistId);
        public static Playlist? NowPlaylist => getPlaylistById(NowPlaylistId);
        public static Playlist? getPlaylistById(string? playlistId)
        {
            int index = getPlaylistIndexById(playlistId);
            if (index != NOTFOUND_INDEX)
            {
                return AllPlaylist[index];
            }
            return null;
        }

        public static MainWindow? mainWindow
        {
            get
            {
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        return window as MainWindow;
                    }
                }
                return null;
            }
        }

        // == METHOD ======================================

        public static void PlayCurrentPlaylist()
        {
            NowPlaylistId = CurrPlaylistId;
            if (CurrPlaylist != null)
            {
                Controller.SetListMedia(CurrPlaylist.ListMedia.ToList());
                Controller.PlayAll();
                IsPlayingRecently = false;
            }
        }

        public static void PlayASongInNewPlaylist(int index)
        {
            NowPlaylistId = CurrPlaylistId;
            Controller.SetListMedia(CurrPlaylist.ListMedia.ToList());
            Controller.PlayMedia(index);
        }

        public static void AddPlaylist(Playlist playlist)
        {
            AllPlaylist.Add(playlist);

            // Persist all playlists
            LocalStorage.saveAllPlaylists();
        }

        public static bool AddMedia(Media media, string playlistId)
        {
            // Thêm vào AllPlaylist
            int index = getPlaylistIndexById(playlistId);
            if (index == NOTFOUND_INDEX) return false;
            bool ok = AllPlaylist[index].AddMedia(media);

            if (!ok) return false;

            // Persist all playlists
            LocalStorage.saveAllPlaylists();

            // Thêm vào PlayerController (nếu là NowPlaylist)
            if (playlistId == NowPlaylistId)
            {
                Controller.ListMedia.Add(media);
            }

            return true;
        }

        public static bool RemoveMedia(Media media, string playlistId)
        {
            // Xóa trong AllPlaylist
            int index = getPlaylistIndexById(playlistId);
            if (index == NOTFOUND_INDEX) return false;
            bool ok = AllPlaylist[index].RemoveMedia(media);

            if (ok == false) return false;

            // Xóa trong PlayerController (nếu là NowPlaylist)
            if (playlistId == NowPlaylistId)
            {
                // Nếu xóa media đang phát, thì phát bài tiếp theo rồi mới xóa
                if (Controller.NowId == media.Id)
                {
                    bool isPlaying = Controller.isPlaying;
                    Controller.PlayNext(forceNext: true);
                    if (!isPlaying) Controller.Pause();
                }
                return Controller.ListMedia.Remove(media);
            }

            return true;
        }

        public static int getPlaylistIndexById(string? playlistId)
        {
            if (playlistId == null) return NOTFOUND_INDEX;

            for (int i = 0; i < AllPlaylist.Count; i++)
            {
                if (AllPlaylist[i].Id == playlistId) return i;
            }

            return NOTFOUND_INDEX;
        }

        public static void UpdateIconPlay()
        {
            mainWindow!.iconPlay.Kind = Controller.IconPlay;
        }

        public static void UpdateSeekingSlider()
        {
            mainWindow!.MediaSeekSlider.Value = Controller.Position;
            mainWindow!.MediaSeekSlider.Maximum = Controller.NowMedia?.Duration ?? 1;
            mainWindow.lblCurrenttime.Text = TimeUtils.FormatTime(Controller.Position);
            mainWindow.lblMusiclength.Text = Controller.NowMedia?.DurationString ?? "0:00";
        }

        public static void UpdateNowPlayingInfo()
        {
            if (IsPlayingRecently)
            {
                mainWindow!.NowPlaylistName.Text = "Recently Played Media";
                mainWindow!.NowMediaName.Text = Controller.NowMedia?.Name ?? "";
            }
            else
            {
                mainWindow!.NowPlaylistName.Text = NowPlaylist?.Name ?? "";
                mainWindow!.NowMediaName.Text = Controller.NowMedia?.Name ?? "";
            }
        }

        public static void UpdateShuffleButton()
        {
            mainWindow!.btnShuffle.Background = Controller.isShuffle ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.Transparent;
            mainWindow!.btnShuffle.Foreground = Controller.isShuffle ? System.Windows.Media.Brushes.Black : System.Windows.Media.Brushes.White;
        }

        public static void UpdateReplayButton()
        {
            mainWindow!.btnReplay.Background = Controller.isReplay ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.Transparent;
            mainWindow!.btnReplay.Foreground = Controller.isReplay ? System.Windows.Media.Brushes.Black : System.Windows.Media.Brushes.White;
        }

        public static void FocusMenuButton(object button)
        {
            if (!(button is ControlButton.Button)) return;
            UnfocusAllMenuButtons();
            (button as ControlButton.Button)!.Background = Utilities.FocusColor;
        }

        public static void UnfocusAllMenuButtons()
        {
            RecentlyPlayedButton.Background = Utilities.UnfocusColor;
            AboutUsButton.Background = Utilities.UnfocusColor;
            for (int i = 0; i < PlayListButton.Count; i++)
            {
                PlayListButton[i].Background = Utilities.UnfocusColor;
            }
        }

        public static Media? getMediaById(string? playlistId, string? mediaId)
        {
            if (playlistId == null || mediaId == null) return null;

            Playlist? playlist = getPlaylistById(playlistId);

            if (playlist == null) return null;

            for (int i = 0; i < playlist.ListMedia.Count; i++)
            {
                if (playlist.ListMedia[i].Id.Equals(mediaId))
                    return playlist.ListMedia[i];
            }

            return null;
        }

        public static int getMediaIndexById(Playlist playlist, string mediaId)
        {
            for (int i = 0; i < playlist.ListMedia.Count; i++)
            {
                if (playlist.ListMedia[i].Id.Equals(mediaId))
                    return i;
            }

            return NOTFOUND_INDEX;
        }
    }
}
