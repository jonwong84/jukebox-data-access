using Jukebox.DataAccess.Contracts.DataContracts.Song;
using Jukebox.DataAccess.EntityFramework.Models;
using Jukebox.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jukebox.DataAccess.Songs
{
    internal class SongRepositoryAccess(EntityFramework.JukeboxDbContext context, ILogger<SongRepositoryAccess> logger) : ISongRepositoryAccess
    {
        private readonly EntityFramework.JukeboxDbContext _context = context;
        private readonly ILogger _logger = logger;

        public async Task<AddSongResult> AddAsync(AddSongRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var song = new Song
                {
                    Title = request.Title,
                    ArtistId = request.ArtistId,
                    AlbumId = request.AlbumId,
                    Duration = request.Duration,
                    TrackNumber = request.TrackNumber,
                    Bpm = request.Bpm,
                };

                if (!string.IsNullOrWhiteSpace(request.Lyrics))
                {
                    song.Lyrics = new SongLyrics
                    {
                        Lyrics = request.Lyrics,
                    };
                }

                await _context.Songs.AddAsync(song, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return new AddSongResult
                {
                    Success = true,
                    SongId = song.Id,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add song with title {Title}", request.Title);
                return new AddSongResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                };
            }
        }

        public Task<bool> DeleteAsync(int songId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<SongSummary?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateSongResult> UpdateAsync(UpdateSongRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
