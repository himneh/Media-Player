using MaterialDesignThemes.Wpf;
using Media_Player.Models;
using System.Windows.Media;
using System.Windows.Threading;

namespace Media_Player.Core
{
    public class PlayerController
    {
        const int NOTFOUND_INDEX = -1;

        // == ATTRIBUTES ======================================

        // Thư viện của Windows
        public MediaPlayer Player = new MediaPlayer();

        // List id các bài đã phát (từ lúc setListMedia), gồm cả bài đang phát
        public List<string> History = new List<string> { };

        // danh sách Media đang chơi
        // lí do là List<Media> mà không phải Playlist:
        //    + không phụ thuộc vào lớp Playlist
        //    + ý nghĩa của PlayerController: phát danh sách bài hát
        public List<Media> ListMedia = new List<Media>();

        // id bài hiện tại đang phát
        public string? NowId { get; set; }

        public PlayerState State = PlayerState.INIT;

        // shuffle mode
        public bool isShuffle = false;

        public bool isReplay = false;

        // Đang kéo seeking bar
        public bool isSeeking = false;

        public PlayerController() {
            Player.MediaEnded += (sender, eventArgs) => PlayNext();
        }


        // == GETTER ======================================

        public int NowIndex => getIndexById(NowId);

        // Trả về bài đang phát
        public Media? NowMedia
        {
            get
            {
                if (NowId == null) return null;
                int index = getIndexById(NowId);
                if (index == NOTFOUND_INDEX) return null;
                return ListMedia[index];
            }
        }

        // Vị trí của bài đang phát (seeking)
        // Đơn vị: second
        public int Position
        {
            get { return (int)Player.Position.TotalSeconds; }
            set
            {
                if (!isInit)
                {
                    // #fix_tạm, #chưa_hiểu
                    Thread.Sleep(50);   // Nếu không delay thì Player.Position sẽ gán 0 sau khi gán value
                    
                    Player.Position = TimeSpan.FromSeconds(value);
                }
            }
        }

        public bool isInit => State == PlayerState.INIT;
        public bool isPlaying => State == PlayerState.PLAYING;
        public bool isPaused => State == PlayerState.PAUSED;
        public bool isComplete => State == PlayerState.COMPLETE;

        public PackIconKind IconPlay => isPlaying ? PackIconKind.Pause : PackIconKind.Play;

        private DispatcherTimer? timer = null;


        // == METHOD ======================================

        public void SetListMedia(List<Media> medias)
        {
            if (State == PlayerState.PLAYING)
            {
                Player.Stop();
            }

            ListMedia.Clear();
            ListMedia.AddRange(medias);

            State = PlayerState.INIT;

            GLOBAL.UpdateIconPlay();
            GLOBAL.UpdateSeekingSlider();

            setUpTimer();

            History.Clear();
        }

        public void PlayAll()
        {
            if (ListMedia.Count == 0) return;

            if (isPlaying || isPaused)
                Player.Stop();
            PlayMedia(0);
            timer?.Start();
        }

        public void Restore(List<Media> medias, string mediaId, int position, List<string> history)
        {
            ListMedia.Clear();
            ListMedia.AddRange(medias);
            NowId = mediaId;
            History.Clear();
            History.AddRange(history);
            State = PlayerState.PAUSED;
            setUpTimer();
            timer?.Start();
            Media media = NowMedia!;
            Player.Open(new Uri(media.FilePath));
            Position = position;
            GLOBAL.UpdateSeekingSlider();
            Player.Play();
            Player.Pause();
            GLOBAL.UpdateNowPlayingInfo();
        }

        public void PlayMedia(int index, int position = 0)
        {
            NowId = getIdByIndex(index);
            if (NowId == null) return;

            if (timer?.IsEnabled != true)
            {
                timer?.Start();
            }

            // Phát media
            Media media = ListMedia[index];
            Player.Open(new Uri(media.FilePath));
            Position = position;
            Player.Play();

            // Cập nhật state
            State = PlayerState.PLAYING;

            // Cập nhật thông tin media đang phát và nút Play
            GLOBAL.UpdateNowPlayingInfo();
            GLOBAL.UpdateIconPlay();

            // Thêm vào lịch sử
            History.Add(NowId);

            // Thêm vào danh sách phát gần đây
            if (!GLOBAL.IsPlayingRecently)
            {
                LocalStorage.addRecentlyMedia(media);
            }

            // Lưu thông tin media cuối (nếu user tắt app thì sau mở lại còn biết)
            LocalStorage.setLastPlayed();

            // Cập nhật thanh seeking
            GLOBAL.mainWindow!.MediaSeekSlider.Maximum = NowMedia!.Duration;
        }

        public void Pause()
        {
            Player.Pause();
            State = PlayerState.PAUSED;
            timer?.Stop();
        }

        public void Continue()
        {
            Player.Play();
            State = PlayerState.PLAYING;
            timer?.Start();
        }

        public void Complete()
        {
            Player.Pause();
            State = PlayerState.COMPLETE;
            timer?.Stop();
            Position = NowMedia?.Duration ?? 0;
            GLOBAL.UpdateIconPlay();
            GLOBAL.UpdateSeekingSlider();
        }

        public void PlayNext(bool forceNext = false)
        {
            // Shuffle Mode
            if (isShuffle)
            {
                int index = getNextRandom();
                PlayMedia(index);
            }
            // Phát bài tiếp theo
            else
            {
                if (NowId == null) return;
                int index = NowIndex;
                if (index == NOTFOUND_INDEX) return;

                if (isReplay && forceNext == false)
                {
                    History.RemoveAt(History.Count - 1);
                    PlayMedia(index);
                }
                else if (index + 1 < ListMedia.Count)
                    PlayMedia(index + 1);
                else
                    Complete();
            }
        }

        public void PlayPrevious()
        {
            if (History.Count == 0) return;

            // Nếu đây là bài đầu tiên, thì phát lại từ đầu media
            if (History.Count == 1)
            {
                PlayMedia(NowIndex, 0);
            }
            // Ngược lại, phát bài trước đó, xóa lịch sử bài hiện tại
            else
            {
                string previousId = History[History.Count - 2];
                int index = getIndexById(previousId);
                History.RemoveAt(History.Count - 1);    // xóa bài hiện tại
                History.RemoveAt(History.Count - 1);    // xóa luôn bài trước đó, vì khi PlayMedia thì kiểu gì cũng thêm lại
                PlayMedia(index);
            }
        }

        public void ToggleShuffle()
        {
            isShuffle = !isShuffle;
            GLOBAL.UpdateShuffleButton();
        }

        public void ToggleReplay()
        {
            isReplay = !isReplay;
            GLOBAL.UpdateReplayButton();
        }

        Random rng = new Random();
        public int getNextRandom()
        {
            if (ListMedia.Count > 1)
            {
                List<int> freePool = Enumerable.Range(0, ListMedia.Count).ToList();
                freePool.Remove(NowIndex);
                return freePool[rng.Next(freePool.Count)];
            }
            else return 0;
        }

        public int getIndexById(string? id)
        {
            if (id == null) return NOTFOUND_INDEX;
            for (int i = 0; i < ListMedia.Count; i++)
            {
                if (ListMedia[i].Id == id)
                {
                    return i;
                }
            }
            return NOTFOUND_INDEX;
        }

        public string? getIdByIndex(int index)
        {
            if (index < 0 || index >= ListMedia.Count)
            {
                return null;
            }
            return ListMedia[index].Id;
        }

        private void setUpTimer()
        {
            if (timer != null) timer.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!isSeeking && isPlaying)
            {
                GLOBAL.UpdateSeekingSlider();
                LocalStorage.setLastPlayedPosition();
            }
        }

        public void DeleteHistoryById(string id)
        {
            int index = -1;

            for (int i = 0; i< History.Count;i++)
            {
                if (History[i].Equals(id))
                    index = i;
            }
            
            if (index != -1) History.RemoveAt(index);
        }
    }
}
