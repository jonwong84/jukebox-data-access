namespace Jukebox.DataAccess.Contracts.DataContracts.Song
{
    public class UpdateSongResult
    {
        public SongDetails? SongDetails { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
