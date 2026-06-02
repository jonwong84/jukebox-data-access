namespace Jukebox.DataAccess.Contracts.DataContracts.Song
{
    public class AddSongResult
    {
        public bool Success { get; set; }
        public int? SongId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
