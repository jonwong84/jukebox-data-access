namespace Jukebox.DataAccess.Contracts.DataContracts.Song
{
    public class DeleteSongResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
