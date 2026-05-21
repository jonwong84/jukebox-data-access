using Jukebox.DataAccess.Contracts.DataContracts.Song;
using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Search
{
    public class SearchResponse
    {
        public List<SongSummary> Songs { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
