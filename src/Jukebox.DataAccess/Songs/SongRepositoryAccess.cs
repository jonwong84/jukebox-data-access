using Jukebox.DataAccess.Contracts.DataContracts.Album;
using Jukebox.DataAccess.Contracts.DataContracts.Artist;
using Jukebox.DataAccess.Contracts.DataContracts.Common;
using Jukebox.DataAccess.Contracts.DataContracts.Song;
using Jukebox.DataAccess.EntityFramework.Models;
using Jukebox.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jukebox.DataAccess.Songs;

public class SongRepositoryAccess(EntityFramework.JukeboxDbContext context, ILogger<SongRepositoryAccess> logger) : ISongRepositoryAccess
{
    private readonly EntityFramework.JukeboxDbContext _context = context;
    private readonly ILogger _logger = logger;

    public async Task<AddSongResult> AddAsync(AddSongRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.GenreIds.Count != 0)
            {
                var invalidGenreIds = await GetInvalidGenreIdsAsync(request.GenreIds, cancellationToken);
                if (invalidGenreIds.Count != 0)
                {
                    return new AddSongResult
                    {
                        Success = false,
                        ErrorMessage = $"The following genre IDs do not exist: {string.Join(", ", invalidGenreIds)}",
                    };
                }
            }

            if (!await ArtistExistsAsync(request.ArtistId, cancellationToken))
            {
                return new AddSongResult
                {
                    Success = false,
                    ErrorMessage = $"Artist with ID {request.ArtistId} does not exist.",
                };
            }

            if (request.AlbumId.HasValue && !await AlbumExistsAsync(request.AlbumId.Value, cancellationToken))
            {
                return new AddSongResult
                {
                    Success = false,
                    ErrorMessage = $"Album with ID {request.AlbumId.Value} does not exist.",
                };
            }

            var song = new Song
            {
                Title = request.Title,
                ArtistId = request.ArtistId,
                AlbumId = request.AlbumId,
                Duration = request.Duration,
                TrackNumber = request.TrackNumber,
                Bpm = request.Bpm,
                SongGenres = request.GenreIds.Select(id => new SongGenre { GenreId = id }).ToList(),
                CreatedBy = request.UserId,
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

    public async Task<DeleteSongResult> DeleteAsync(int songId, CancellationToken cancellationToken = default)
    {
        try
        {
            var song = await _context.Songs
                .FirstOrDefaultAsync(s => s.Id == songId, cancellationToken);

            if (song is null)
            {
                return new DeleteSongResult
                {
                    Success = false,
                    ErrorMessage = $"Song with ID {songId} was not found.",
                };
            }

            _context.Songs.Remove(song);
            await _context.SaveChangesAsync(cancellationToken);

            return new DeleteSongResult
            {
                Success = true,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete song with ID {SongId}", songId);
            return new DeleteSongResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<GetSongResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var song = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Album)
                    .ThenInclude(a => a!.AlbumArtists)
                        .ThenInclude(aa => aa.Artist)
                .Include(s => s.SongGenres)
                    .ThenInclude(sg => sg.Genre)
                .Include(s => s.Lyrics)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (song is null)
            {
                return new GetSongResult
                {
                    Success = false,
                    ErrorMessage = $"Song with ID {id} was not found.",
                };
            }

            return new GetSongResult
            {
                Success = true,
                SongDetails = MapToSongDetails(song),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve song with ID {SongId}", id);
            return new GetSongResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<UpdateSongResult> UpdateAsync(UpdateSongRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var song = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Album)
                    .ThenInclude(a => a!.AlbumArtists)
                        .ThenInclude(aa => aa.Artist)
                .Include(s => s.SongGenres)
                    .ThenInclude(sg => sg.Genre)
                .Include(s => s.Lyrics)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (song is null)
            {
                return new UpdateSongResult
                {
                    Success = false,
                    ErrorMessage = $"Song with ID {request.Id} was not found.",
                };
            }

            if (request.GenreIds.Count != 0)
            {
                var invalidGenreIds = await GetInvalidGenreIdsAsync(request.GenreIds, cancellationToken);
                if (invalidGenreIds.Count != 0)
                {
                    return new UpdateSongResult
                    {
                        Success = false,
                        ErrorMessage = $"The following genre IDs do not exist: {string.Join(", ", invalidGenreIds)}",
                    };
                }
            }

            if (!await ArtistExistsAsync(request.ArtistId, cancellationToken))
            {
                return new UpdateSongResult
                {
                    Success = false,
                    ErrorMessage = $"Artist with ID {request.ArtistId} does not exist.",
                };
            }

            if (request.AlbumId.HasValue && !await AlbumExistsAsync(request.AlbumId.Value, cancellationToken))
            {
                return new UpdateSongResult
                {
                    Success = false,
                    ErrorMessage = $"Album with ID {request.AlbumId.Value} does not exist.",
                };
            }

            song.Title = request.Title;
            song.ArtistId = request.ArtistId;
            song.AlbumId = request.AlbumId;
            song.Duration = request.Duration;
            song.TrackNumber = request.TrackNumber;
            song.Bpm = request.Bpm;
            song.UpdatedBy = request.UserId;

            if (!string.IsNullOrWhiteSpace(request.Lyrics))
            {
                if (song.Lyrics is null)
                {
                    song.Lyrics = new SongLyrics { Lyrics = request.Lyrics };
                }
                else
                {
                    song.Lyrics.Lyrics = request.Lyrics;
                }
            }
            else if (song.Lyrics is not null)
            {
                _context.Remove(song.Lyrics);
            }

            song.SongGenres = request.GenreIds
                .Select(id => new SongGenre { SongId = song.Id, GenreId = id })
                .ToList();

            await _context.SaveChangesAsync(cancellationToken);

            await _context.Entry(song)
                .Collection(s => s.SongGenres)
                .Query()
                .Include(sg => sg.Genre)
                .LoadAsync(cancellationToken);

            return new UpdateSongResult
            {
                Success = true,
                SongDetails = MapToSongDetails(song),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update song with ID {SongId}", request.Id);
            return new UpdateSongResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    private static SongDetails MapToSongDetails(Song song) => new()
    {
        Id = song.Id,
        Title = song.Title,
        ArtistId = song.ArtistId,
        Artist = new ArtistSummary
        {
            Id = song.Artist.Id,
            Name = song.Artist.Name,
        },
        AlbumId = song.AlbumId,
        Album = song.Album is null ? null : new AlbumSummary
        {
            Id = song.Album.Id,
            Title = song.Album.Title,
            Artists = song.Album.AlbumArtists
                .Select(aa => new ArtistSummary
                {
                    Id = aa.Artist.Id,
                    Name = aa.Artist.Name,
                })
                .ToList(),
        },
        Duration = song.Duration,
        Genres = song.SongGenres
            .Select(sg => new GenreSummary
            {
                Id = sg.Genre.Id,
                Name = sg.Genre.Name,
            })
            .ToList(),
        TrackNumber = song.TrackNumber,
        Bpm = song.Bpm,
        Lyrics = song.Lyrics?.Lyrics ?? string.Empty,
    };

    private async Task<List<int>> GetInvalidGenreIdsAsync(List<int> genreIds, CancellationToken cancellationToken)
    {
        var existingGenreIds = await _context.Genres
            .Where(g => genreIds.Contains(g.Id))
            .Select(g => g.Id)
            .ToListAsync(cancellationToken);

        return genreIds.Except(existingGenreIds).ToList();
    }

    private async Task<bool> ArtistExistsAsync(int artistId, CancellationToken cancellationToken) =>
        await _context.Artists.AnyAsync(a => a.Id == artistId, cancellationToken);

    private async Task<bool> AlbumExistsAsync(int albumId, CancellationToken cancellationToken) =>
        await _context.Albums.AnyAsync(a => a.Id == albumId, cancellationToken);

    public async Task<ListSongsResult> ListAsync(ListSongsRequest request, CancellationToken cancellationToken = default)
    {
        var pageSize = Math.Min(request.PageSize, 100);
        var pageNumber = Math.Max(request.PageNumber, 1);

        var query = _context.Songs
            .Include(s => s.Artist)
            .Include(s => s.Album)
                .ThenInclude(a => a!.AlbumArtists)
                    .ThenInclude(aa => aa.Artist)
            .AsQueryable();

        if (request.ArtistId.HasValue)
            query = query.Where(s => s.ArtistId == request.ArtistId.Value);

        if (request.AlbumId.HasValue)
            query = query.Where(s => s.AlbumId == request.AlbumId.Value);

        if (request.GenreId.HasValue)
            query = query.Where(s => s.SongGenres.Any(sg => sg.GenreId == request.GenreId.Value));

        if (request.MinBpm.HasValue)
            query = query.Where(s => s.Bpm >= request.MinBpm.Value);

        if (request.MaxBpm.HasValue)
            query = query.Where(s => s.Bpm <= request.MaxBpm.Value);

        if (!string.IsNullOrWhiteSpace(request.TitleSearch))
            query = query.Where(s => s.Title.Contains(request.TitleSearch));

        var totalCount = await query.CountAsync(cancellationToken);

        var songs = await query
            .OrderBy(s => s.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ListSongsResult
        {
            Success = true,
            Songs = songs.Select(s => new SongSummary
            {
                Id = s.Id,
                Title = s.Title,
                Artist = s.Artist.Name,
                Album = s.Album?.Title
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
