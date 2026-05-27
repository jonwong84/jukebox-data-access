# Jukebox.DataAccess.Migrations

This project is responsible for managing EF Core migrations that apply model changes to the SQL database schemas for the Jukebox API.

## How To Get Started

### Prerequisites

This section covers everything you need to execute migrations.

#### EF Core CLI

If you don't already have the EF Core CLI tools set up, do the following:

- Run this command in a terminal:
  - `bashdotnet tool install --global dotnet-ef`
- Verify it installed correctly with:
  - `bashdotnet ef`

#### SQL Server

You will need 

You will need a SQL server to work with. You can set one up locally on your machine but the easiest way to get started is by spinning one up in Docker.

- In Powershell, run this Docker command to spin up a new SQL server:
  - `docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" -p 1433:1433 --name my_sql_server -d mcr.microsoft.com/mssql/server:2022-latest`

- Now set your connection string as an environmental variable. In Powershell, run this command:
  - `[System.Environment]::SetEnvironmentVariable("JUKEBOX_DB_CONNECTION_STRING", "Server=localhost,1433;Database=Jukebox;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;", "User")`

- Run `Get-ChildItem Env: | Sort-Object Name` to list your environmental variables and confirm that it was successfully created.

---
### Running Migrations - Starting From Scratch

- Run this command from the `jukebox-data-access` repo root to create the initial migration:
  - `bashdotnet ef migrations add InitialCreate --project src/Jukebox.DataAccess.Migrations/Jukebox.DataAccess.Migrations.csproj --startup-project src/Jukebox.DataAccess.Migrations/Jukebox.DataAccess.Migrations.csproj`
  - This tells the EF Core CLI to:
    - Create a migration named `InitialCreate`
    - Use `Jukebox.DataAccess.Migrations` as both the migrations project and the startup project since that's where the `IDesignTimeDbContextFactory` lives