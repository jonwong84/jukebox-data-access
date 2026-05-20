namespace Jukebox.DataAccess.Contracts.DataContracts.Song
{
    public class UpdateSongResult
    {
        public SongSummary? Song { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
