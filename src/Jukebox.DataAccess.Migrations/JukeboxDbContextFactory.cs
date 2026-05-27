using Jukebox.DataAccess.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jukebox.DataAccess.Migrations;

public class JukeboxDbContextFactory : IDesignTimeDbContextFactory<JukeboxDbContext>
{
    public JukeboxDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<JukeboxDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("JUKEBOX_DB_CONNECTION_STRING")
            ?? throw new InvalidOperationException("The environment variable 'JUKEBOX_DB_CONNECTION_STRING' is not set.");

        optionsBuilder.UseSqlServer(connectionString);

        return new JukeboxDbContext(optionsBuilder.Options);
    }
}