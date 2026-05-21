using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Search
{
    public class SearchRequest
    {
        public string? Title { get; set; }
        public int? ArtistId { get; set; }
        public string? Album { get; set; }
        public List<int>? GenreIds { get; set; }
        public int? Year { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
