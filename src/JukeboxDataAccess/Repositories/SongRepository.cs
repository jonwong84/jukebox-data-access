using System.Data;
using Dapper;
using JukeboxDataAccess.DataModels;
using JukeboxDataAccess.Interfaces;
using JukeboxDataAccess.Models;
using JukeboxDataAccess.Requests;

namespace JukeboxDataAccess.Repositories;

/// <summary>
/// Provides SQL-backed data access operations for <see cref="Song"/> entities using Dapper.
/// </summary>
public class SongRepository : ISongRepository
{
    private readonly IDbConnection _connection;
    private readonly IMapper<SongDataModel, Song> _mapper;

    /// <summary>
    /// Initialises a new instance of <see cref="SongRepository"/>.
    /// </summary>
    /// <param name="connection">An open (or lazily-opened) database connection.</param>
    /// <param name="mapper">The mapper used to convert database rows to domain models.</param>
    public SongRepository(IDbConnection connection, IMapper<SongDataModel, Song> mapper)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(mapper);

        _connection = connection;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<Song?> GetByIdAsync(GetSongRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        const string sql = """
            SELECT Id, Title, Artist, Album, Genre, DurationSeconds, ReleaseYear
            FROM Songs
            WHERE Id = @Id
            """;

        var dataModel = await _connection.QuerySingleOrDefaultAsync<SongDataModel>(sql, new { request.Id });
        return dataModel is null ? null : _mapper.Map(dataModel);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Song>> SearchAsync(SearchSongsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var sql = """
            SELECT Id, Title, Artist, Album, Genre, DurationSeconds, ReleaseYear
            FROM Songs
            WHERE 1 = 1
            """;

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            sql += " AND Title LIKE @Title ESCAPE '\\'";
            parameters.Add("Title", BuildLikePattern(request.Title));
        }

        if (!string.IsNullOrWhiteSpace(request.Artist))
        {
            sql += " AND Artist LIKE @Artist ESCAPE '\\'";
            parameters.Add("Artist", BuildLikePattern(request.Artist));
        }

        if (!string.IsNullOrWhiteSpace(request.Album))
        {
            sql += " AND Album LIKE @Album ESCAPE '\\'";
            parameters.Add("Album", BuildLikePattern(request.Album));
        }

        if (!string.IsNullOrWhiteSpace(request.Genre))
        {
            sql += " AND Genre = @Genre";
            parameters.Add("Genre", request.Genre);
        }

        if (request.ReleaseYear.HasValue)
        {
            sql += " AND ReleaseYear = @ReleaseYear";
            parameters.Add("ReleaseYear", request.ReleaseYear.Value);
        }

        var dataModels = await _connection.QueryAsync<SongDataModel>(sql, parameters);
        return dataModels.Select(_mapper.Map);
    }

    /// <summary>
    /// Wraps <paramref name="value"/> in SQL LIKE wildcard characters after escaping any
    /// literal <c>%</c>, <c>_</c>, and <c>\</c> characters using the backslash escape convention.
    /// </summary>
    private static string BuildLikePattern(string value) =>
        "%" + value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
}
