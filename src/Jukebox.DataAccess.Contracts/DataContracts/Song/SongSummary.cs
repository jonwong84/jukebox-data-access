namespace Jukebox.DataAccess.Contracts.DataContracts.Song
{
    public class SongSummary
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
    }
}
