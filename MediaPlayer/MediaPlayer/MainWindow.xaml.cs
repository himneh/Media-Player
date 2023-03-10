using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ControlButton = System.Windows.Controls;
using Media_Player.Core;
using Media_Player.Models;
using System.Xml;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Diagnostics;
using Gma.System.MouseKeyHook;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json.Linq;
using MaterialDesignColors.Recommended;
using System.Windows.Media;


namespace Media_Player
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        const string BUTTON_NAME = "button";
        double volume_value = 100;
        private IKeyboardMouseEvents _hook;
        Boolean isCreatedPlaylistDialogOpened= false;

        public event PropertyChangedEventHandler PropertyChanged;

        void RaiseChangeEvent([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected virtual void OnPropertyChanged(string newValue)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(newValue));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Volume_Slider.Value = 100;
            GLOBAL.Controller.Player.Volume = Volume_Slider.Value / 100;
            Subscribe();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GLOBAL.RecentlyPlayedButton = btn_recently_played;
            GLOBAL.AboutUsButton = btn_about_us;
            btn_about_us.RaiseEvent(new RoutedEventArgs(ControlButton.Button.ClickEvent));

            // Khôi phục tất cả playlist
            RestoreAllPlaylists();

            // Khôi phục lần chơi cuối
            RestoreLastPlayed();
        }

        private void RestoreAllPlaylists()
        {
            GLOBAL.AllPlaylist.Clear();
            GLOBAL.AllPlaylist.AddRange(LocalStorage.getAllPlaylists());
            if (GLOBAL.AllPlaylist.Count == 0) return;
            foreach (Playlist playlist in GLOBAL.AllPlaylist)
            {
                setUpPlaylist(playlist);
            }
        }
        private void RestoreLastPlayed()
        {
            string? playlistId = LocalStorage.getLastPlayedPlaylistId();
            if (playlistId == null) return;
            string? mediaId = LocalStorage.getLastPlayedMediaId();
            if (mediaId == null) return;
            int position = LocalStorage.getLastPlayedPosition() ?? 0;
            List<string> history = LocalStorage.getLastPlayedHistory() ?? new List<string>();

            Debug.WriteLine($"RestoreLastPlayed: playlistId={playlistId}, mediaId={mediaId}, position={position}, history={history}");

            Playlist? playlist = GLOBAL.getPlaylistById(playlistId);
            if (playlist == null) return;
            GLOBAL.NowPlaylistId = playlistId;
            GLOBAL.Controller.Restore(playlist.ListMedia.ToList(), mediaId, position, history);
        }

        private void btn_now_playing_Click(object sender, RoutedEventArgs e)
        {
            if (GLOBAL.NowPlaylistId != null)
            {
                for (int i = 0; i < GLOBAL.PlayListButton.Count; i++)
                {
                    string name = GLOBAL.PlayListButton[i].Name.ToString();

                    string[] words = name.Split(BUTTON_NAME);
                    string id = words[1];

                    if (id.Equals(GLOBAL.NowPlaylistId))
                    {
                        GLOBAL.PlayListButton[i].RaiseEvent(new RoutedEventArgs(ControlButton.Button.ClickEvent));
                        break;
                    }
                }
            }
        }

        private void btn_create_playlist_Click(object sender, RoutedEventArgs e)
        {
            var screen = new CreatePlayListDialog();
            isCreatedPlaylistDialogOpened = true;
            if (screen.ShowDialog() == true)
            {
                // create new PlayList
                Playlist playlist = new Playlist();
                playlist.Name = screen.playlistName;

                GLOBAL.AddPlaylist(playlist);

                setUpPlaylist(playlist, isOpen: true);
                isCreatedPlaylistDialogOpened = false;
            }
        }

        // set up button playlist, store to state, nav
        private void setUpPlaylist(Playlist playlist, bool isOpen = false)
        {
            // add the PlayList to Stack Panel PlayList
            ControlButton.Button playListButton = new ControlButton.Button();
            playListButton.Content = playlist.Name;
            playListButton.Name = BUTTON_NAME + playlist.Id;
            playListButton.Style = this.FindResource("button") as Style;
            playListButton.Click += aPlayList_Click;

            GLOBAL.PlayListButton.Add(playListButton);
            this.StackPanelPlayList.Children.Add(playListButton);

            if (isOpen)
            {
                GLOBAL.CurrPlaylistId = playlist.Id;
                GLOBAL.FocusMenuButton(playListButton);
                HostPageInFrame(this.MainFrame, new PlayListPage());
            }
        }

        private void aPlayList_Click(object sender, RoutedEventArgs e)
        {
            string name = (sender as ControlButton.Button).Name.ToString();

            string[] words = name.Split(BUTTON_NAME);
            string id = words[1];

            // set currPlayListId
            GLOBAL.CurrPlaylistId = id;

            // open new PlayListPage in Main Frame
            HostPageInFrame(this.MainFrame, new PlayListPage());

            GLOBAL.FocusMenuButton(sender);
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            GLOBAL.UpdateNowPlayingInfo();
            HandlePlay();
        }

        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            HandleNext();
        }

        private void Prev_Button_Click(object sender, RoutedEventArgs e)
        {
            HandlePrevious();
        }

        public void performClickPlay()
        {
            btnPlay.RaiseEvent(new RoutedEventArgs(ControlButton.Button.ClickEvent));
        }

        private void btn_recently_played_Click(object sender, RoutedEventArgs e)
        {
            HostPageInFrame(this.MainFrame, new RecentlyMediaPage());
            GLOBAL.CurrPlaylistId = null;
            GLOBAL.FocusMenuButton(sender);

        }

        private void btn_about_us_Click(object sender, RoutedEventArgs e)
        {
            HostPageInFrame(this.MainFrame, new LandingPage());
            GLOBAL.CurrPlaylistId = null;
            GLOBAL.FocusMenuButton(sender);
        }

        private void Volume_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = (sender as Slider).Value;
            GLOBAL.Controller.Player.Volume = value / 100;
            if (value == 0)
            {
                Volumn_Mode_Btn.IsChecked = true;
            }
            else
            {
                Volumn_Mode_Btn.IsChecked = false;
                //volume_value = value;
            }
            Debug.WriteLine("MEDIA VOLUME " + GLOBAL.Controller.Player.Volume.ToString());
        }

        private void Volumn_Mode_Btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleButton;
            var value = Volume_Slider.Value;
            Debug.WriteLine("Check " + btn.IsChecked.ToString());
            if (btn.IsChecked == true)
            {
                if (value == 0) volume_value = 100;
                else volume_value = value;
                Debug.WriteLine("Volume:" + volume_value.ToString());
                Volume_Slider.Value = 0;
            }
            else
            {
                if (volume_value == 0) Volume_Slider.Value = 100;
                Volume_Slider.Value = volume_value;
            }
        }

        public void HandleNext()
        {
            GLOBAL.Controller.PlayNext(forceNext: true);
        }

        public void HandlePrevious()
        {
            GLOBAL.Controller.PlayPrevious();
        }

        public void HandlePlay()
        {
            switch (GLOBAL.Controller.State)
            {
                case PlayerState.INIT:
                    Playlist? currPlaylist = GLOBAL.CurrPlaylist;
                    if (currPlaylist != null)
                    {
                        GLOBAL.Controller.SetListMedia(currPlaylist.ListMedia.ToList());
                        GLOBAL.NowPlaylistId = currPlaylist.Id;
                        GLOBAL.Controller.PlayAll();
                    }
                    break;
                case PlayerState.PLAYING:
                    GLOBAL.Controller.Pause();
                    break;
                case PlayerState.PAUSED:
                    GLOBAL.Controller.Continue();
                    break;
                case PlayerState.COMPLETE:
                    GLOBAL.Controller.PlayAll();
                    break;
            }
            GLOBAL.UpdateIconPlay();
        }

        public void HostPageInFrame(Frame frame, Page page)
        {
            frame.Content = page;
        }

        private void MediaSeekSlider_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (GLOBAL.Controller.isInit) return;
            GLOBAL.Controller.isSeeking = true;
        }

        private void MediaSeekSlider_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (GLOBAL.Controller.isInit) return;
            GLOBAL.Controller.isSeeking = false;
            GLOBAL.Controller.Position = (int)MediaSeekSlider.Value;
            GLOBAL.UpdateSeekingSlider();
            LocalStorage.setLastPlayedPosition();
        }

        private void btn_open_files_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Playlist Files|*.plf";
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadPlaylist(open.FileName);
            }
        }

        private void LoadPlaylist(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);

            var root = doc.DocumentElement;
            var MediaList = root.ChildNodes[0];

            Playlist playlist = new Playlist();
            playlist.Name = Path.GetFileNameWithoutExtension(path);

            foreach (XmlNode e in MediaList)
            {
                Media media = new Media(e.Attributes["FilePath"].Value, e.Attributes["Id"].Value);
                playlist.ListMedia.Add(media);
            }

            GLOBAL.AddPlaylist(playlist);

            setUpPlaylist(playlist);
        }

        private void Shuffle_Button_Click(object sender, RoutedEventArgs e)
        {
            GLOBAL.Controller.ToggleShuffle();
        }

        private void Replay_Button_Click(object sender, RoutedEventArgs e)
        {
            GLOBAL.Controller.ToggleReplay();
        }

        
        public void Subscribe()
        {
            _hook = Hook.GlobalEvents();
            _hook.KeyDown += _hook_KeyDown;
        }

        private void _hook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == System.Windows.Forms.Keys.M)
            {
                Volumn_Mode_Btn.IsChecked = !Volumn_Mode_Btn.IsChecked;
                Volumn_Mode_Btn_Click(Volumn_Mode_Btn, new RoutedEventArgs());
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.Down)
            {
                Volume_Slider.Value -= 5;
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.Up)
            {
                Volume_Slider.Value += 5;
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.Right)
            {
                btnPNext.RaiseEvent(new RoutedEventArgs(ControlButton.Button.ClickEvent, btnPNext));
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.Left)
            {
                btnPrewind.RaiseEvent(new RoutedEventArgs(ControlButton.Button.ClickEvent, btnPrewind));
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.N)
            {
                btn_create_playlist.RaiseEvent(new RoutedEventArgs(ControlButton.Button.ClickEvent, btn_create_playlist));
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.S)
            {
                if (GLOBAL.CurrPlaylistId == null) return;
                if (GLOBAL.CurrPlaylist?.ListMedia.Count == 0) return;

                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.DefaultExt = "plf";
                saveFile.FileName = GLOBAL.CurrPlaylist.Name;
                saveFile.Filter = "Playlist Files|*.plf";

                if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SavePlaylist(saveFile.FileName);
                }
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.Space)
            {
                if(!isCreatedPlaylistDialogOpened) 
                    btnPlay.RaiseEvent(new RoutedEventArgs(ControlButton.Button.ClickEvent, btnPlay));
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
        public void Unsubscribe()
        {
            _hook.KeyDown -= _hook_KeyDown;
            _hook.Dispose();
        }
    }
}
