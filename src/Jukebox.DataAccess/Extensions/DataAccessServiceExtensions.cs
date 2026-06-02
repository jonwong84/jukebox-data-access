using Jukebox.DataAccess.EntityFramework;
using Jukebox.DataAccess.Interfaces;
using Jukebox.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jukebox.DataAccess.Extensions;

public static class DataAccessServiceExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable("JUKEBOX_DB_CONNECTION_STRING")
            ?? throw new InvalidOperationException("The environment variable 'JUKEBOX_DB_CONNECTION_STRING' is not set.");

        services.AddDbContext<JukeboxDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ISongRepositoryAccess, SongRepositoryAccess>();
        services.AddScoped<IArtistRepositoryAccess, ArtistRepositoryAccess>();
        services.AddScoped<IAlbumRepositoryAccess, AlbumRepositoryAccess>();

        return services;
    }
}