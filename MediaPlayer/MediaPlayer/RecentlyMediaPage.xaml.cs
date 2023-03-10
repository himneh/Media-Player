using Media_Player.Core;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;

namespace Media_Player
{
    public partial class RecentlyMediaPage : Page, INotifyPropertyChanged
    {
        private List<Media> _listMedia;

        public RecentlyMediaPage()
        {
            InitializeComponent();
            _listMedia = LocalStorage.getRecentlyMedia();
            List.ItemsSource = _listMedia;

            DataContext = this;
        }

        public int Total_tracks => _listMedia.Count;

        public string Total_duration
        {
            get
            {
                Playlist tmpPlaylist = new Playlist();
                tmpPlaylist.ListMedia = new BindingList<Media>(_listMedia);
                return tmpPlaylist.TotalDurationString ?? "0:00:00";
            }
        }

        public void RefreshPage()
        {
            List.ItemsSource = _listMedia;
            List.Items.Refresh();
            DataContext = null;
            DataContext = this;
        }

        private void playAllButton_Click(object sender, RoutedEventArgs e)
        {
            GLOBAL.Controller.SetListMedia(LocalStorage.getRecentlyMedia());
            GLOBAL.IsPlayingRecently = true;
            GLOBAL.Controller.PlayAll();
            GLOBAL.UpdateIconPlay();
        }

        private void clearAllButton_Click(object sender, RoutedEventArgs e)
        {
            LocalStorage.clearRecentlyMedia();
            _listMedia.Clear();
            RefreshPage();
            GLOBAL.Controller.SetListMedia(_listMedia);
            GLOBAL.IsPlayingRecently = false;
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Media? media = (sender as Button)?.DataContext as Media;
            if (media != null)
            {
                LocalStorage.removeRecentlyMedia(media);
                _listMedia.Remove(media);
                RefreshPage();

                // Xóa trong PlayerController
                // Nếu xóa media đang phát, thì phát bài tiếp theo rồi mới xóa
                if (GLOBAL.Controller.NowId == media.Id)
                {
                    bool isPlaying = GLOBAL.Controller.isPlaying;
                    GLOBAL.Controller.PlayNext(forceNext: true);
                    if (!isPlaying) GLOBAL.Controller.Pause();
                }
                GLOBAL.Controller.ListMedia.Remove(media);
            }
        }
    }
}
