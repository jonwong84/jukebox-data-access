using System;
using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Song
{
    public class AddSongRequest
    {
        public string Title { get; set; } = string.Empty;
        public int ArtistId { get; set; }
        public int? AlbumId { get; set; }
        public TimeSpan Duration { get; set; }
        public List<int> GenreIds { get; set; } = [];
        public int? TrackNumber { get; set; }
        public int? Bpm { get; set; }
        public string Lyrics { get; set; } = string.Empty;
    }
}
