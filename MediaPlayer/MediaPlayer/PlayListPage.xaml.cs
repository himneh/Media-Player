using Media_Player.Core;
using Media_Player.Utils;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.IO;
using System.Windows.Shapes;
using System;
using Button = System.Windows.Controls.Button;
using ControlButton = System.Windows.Controls;

namespace Media_Player
{
    public partial class PlayListPage : Page, INotifyPropertyChanged
    {
        public PlayListPage()
        {
            InitializeComponent();

            List.ItemsSource = GLOBAL.CurrPlaylist?.ListMedia;
            Playlist_Name.Text = GLOBAL.CurrPlaylist?.Name;

            DataContext = this;
        }

        public string Playlist_name => GLOBAL.CurrPlaylist?.Name ?? "";

        public int Total_tracks => GLOBAL.CurrPlaylist?.ListMedia.Count ?? 0;

        public string Total_duration => GLOBAL.CurrPlaylist?.TotalDurationString ?? "0:00:00";

        public void RefreshPage()
        {
            DataContext = null;
            DataContext = this;
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog open = new Microsoft.Win32.OpenFileDialog
            {
                Filter = FileUtils.MEDIA_FILTER,
                Multiselect = true
            };
            if (open.ShowDialog() == true)
            {
                string[] filepaths = open.FileNames;

                for (int i = 0; i < filepaths.Length; i++)
                {
                    if (FileUtils.IsMedia(filepaths[i]))
                    {
                        string mediaId = GLOBAL.CurrPlaylistId + '_' + filepaths[i];
                        Media media = new Media(filepaths[i], mediaId);
                        AddNewMedia(media);
                    }
                }
            }
        }

        private void playAllPLButton_Click(object sender, RoutedEventArgs e)
        {
            GLOBAL.PlayCurrentPlaylist();
            GLOBAL.UpdateIconPlay();
        }

        private void Playlist_Name_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Btn_SavePlaylist(object sender, RoutedEventArgs e)
        {
            if (GLOBAL.CurrPlaylist?.ListMedia.Count == 0) return;

            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.DefaultExt = "plf";
            saveFile.FileName = GLOBAL.CurrPlaylist.Name;
            saveFile.Filter = "Playlist Files|*.plf";

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                SavePlaylist(saveFile.FileName);
            }
        }

        void SavePlaylist(string Path)
        {
            XmlDocument doc = new XmlDocument();

            var root = doc.CreateElement("MediaPlayer");
            var mediaList = doc.CreateElement("Playlist");

            doc.AppendChild(root);
            root.AppendChild(mediaList);

            foreach (var e in GLOBAL.CurrPlaylist.ListMedia)
            {
                var mediaNode = doc.CreateElement("Media");
                mediaNode.SetAttribute("Id", e.Id);
                mediaNode.SetAttribute("FilePath", e.FilePath);
                mediaList.AppendChild(mediaNode);
            }
            doc.Save(Path);

            //if (Directory.GetDirectories(Path) != null)
            //{
            //    System.Windows.Forms.MessageBox.Show("Lưu thành công, đường dẫn: " + Path, "Thông Báo");
            //}
            System.Windows.Forms.MessageBox.Show("Lưu thành công, đường dẫn: " + Path, "Thông Báo");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void RaiseChangeEvent([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        void AddNewMedia(Media media)
        {
            if (GLOBAL.CurrPlaylistId != null)
            {
                GLOBAL.AddMedia(media, GLOBAL.CurrPlaylistId);
                RefreshPage();
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Media? media = (sender as Button)?.DataContext as Media;
            if (media != null)
            {
                if (GLOBAL.CurrPlaylistId != null)
                {
                    GLOBAL.RemoveMedia(media, GLOBAL.CurrPlaylistId);
                    LocalStorage.clearLastPlayed();
                    RefreshPage();
                    GLOBAL.UpdateNowPlayingInfo();
                    GLOBAL.UpdateSeekingSlider();
                    GLOBAL.Controller.DeleteHistoryById(media.Id);
                    LocalStorage.saveAllPlaylists();
                }

            }
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (List.SelectedItems.Count > 0)
            {
                Media currMediaClicked = (Media)List.SelectedItems[0];

                if (GLOBAL.NowPlaylist != null && GLOBAL.CurrPlaylist.Id.Equals(GLOBAL.NowPlaylist.Id))
                {
                    // lấy index của bài hát clicked trong playlist đang phát                    
                    int index = GLOBAL.getMediaIndexById(GLOBAL.NowPlaylist, currMediaClicked.Id);

                    // ko cần check vì hàm PlayMedia có check r
                    GLOBAL.Controller.PlayMedia(index);
                }
                else
                {
                    // lấy index của bài hát clicked trong curr playlist
                    int index = GLOBAL.getMediaIndexById(GLOBAL.CurrPlaylist, currMediaClicked.Id);

                    GLOBAL.PlayASongInNewPlaylist(index);
                }
            }
        }
    }

}
