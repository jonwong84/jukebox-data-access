using Jukebox.DataAccess.Contracts.DataContracts.Artist;
using System;
using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Album
{
    public class AlbumSummary
    {
        public int Id { get; set; }
        public required string Title { get; set; } = String.Empty;
        public List<ArtistSummary> Artists { get; set; } = new List<ArtistSummary>();
    }
}
