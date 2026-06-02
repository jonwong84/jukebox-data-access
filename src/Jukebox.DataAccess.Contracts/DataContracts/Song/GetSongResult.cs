namespace Jukebox.DataAccess.Contracts.DataContracts.Song
{
    public class GetSongResult
    {
        public bool Success { get; set; }
        public SongDetails? SongDetails { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
