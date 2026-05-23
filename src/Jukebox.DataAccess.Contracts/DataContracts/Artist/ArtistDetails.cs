using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Artist
{
    public class ArtistDetails
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<int> GenreIds { get; set; } = [];
        public List<int> AlbumIds { get; set; } = [];
        public string Bio { get; set; } = string.Empty;
    }
}
