using System;
using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Song
{
    public class UpdateSongRequest
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ArtistId { get; set; }
        public int? AlbumId { get; set; }
        public TimeSpan Duration { get; set; }
        public int? TrackNumber { get; set; }
        public int? Bpm { get; set; }
        public string Lyrics { get; set; } = string.Empty;
        public List<int> GenreIds { get; set; } = [];
        public string? UserId { get; set; }
    }
}
