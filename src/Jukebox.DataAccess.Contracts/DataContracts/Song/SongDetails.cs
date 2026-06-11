using Jukebox.DataAccess.Contracts.DataContracts.Album;
using Jukebox.DataAccess.Contracts.DataContracts.Artist;
using Jukebox.DataAccess.Contracts.DataContracts.Genre;
using System;
using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Song
{
    public class SongDetails
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public int ArtistId { get; set; }
        public ArtistSummary Artist { get; set; } = null!;
        public int? AlbumId { get; set; }
        public AlbumSummary? Album { get; set; }
        public TimeSpan Duration { get; set; }
        public ICollection<GenreSummary> Genres { get; set; } = new List<GenreSummary>();
        public int? TrackNumber { get; set; }
        public int? Bpm { get; set; }
        public string Lyrics { get; set; } = string.Empty;
    }
}
