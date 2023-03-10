namespace Media_Player.Models
{
    public enum PlayerState
    {
        INIT,    // chưa phát bài nào (hoặc khi xảy ra lỗi phát nhạc)
        PLAYING,    // đang phát
        PAUSED,     // đang tạm dừng
        COMPLETE,	// đã phát hết playlist
    }
}
