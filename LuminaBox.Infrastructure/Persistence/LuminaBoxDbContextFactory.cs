using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LuminaBox.Infrastructure.Persistence
{
    /// <summary>
    /// Design-time factory used only by the EF Core tools (<c>dotnet ef migrations ...</c>,
    /// <c>dotnet ef database update</c>) to create <see cref="LuminaBoxDbContext"/> without
    /// booting the API host. It is never used at runtime; the API registers the context
    /// through <c>AddDbContext</c> in <c>IocContainer.SetupDbContext</c>.
    /// </summary>
    /// <remarks>
    /// Configuration is read from the API project, in order of increasing priority:
    /// <c>appsettings.json</c> (required), <c>appsettings.{ASPNETCORE_ENVIRONMENT}.json</c> (optional),
    /// User Secrets of the API project (optional), then environment variables
    /// (e.g. <c>ConnectionStrings__DefaultConnection</c>).
    /// </remarks>
    public class LuminaBoxDbContextFactory : IDesignTimeDbContextFactory<LuminaBoxDbContext>
    {
        private const string ApiProjectName = "LuminaBox.Api";
        private const string ConnectionStringName = "DefaultConnection";
        private const string ApiDirectoryOverrideVariable = "LUMINABOX_API_DIR";

        public LuminaBoxDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var apiDirectory = ResolveApiDirectory();

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(apiDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true);

            var userSecretsId = ReadUserSecretsId(apiDirectory);
            if (userSecretsId is not null)
            {
                configurationBuilder.AddUserSecrets(userSecretsId);
            }

            var configuration = configurationBuilder
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString(ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{ConnectionStringName}' is missing for environment '{environment}'. " +
                    $"Set 'ConnectionStrings:{ConnectionStringName}' in {Path.Combine(apiDirectory, "appsettings.json")}, " +
                    $"appsettings.{environment}.json, User Secrets of {ApiProjectName}, " +
                    $"or the environment variable 'ConnectionStrings__{ConnectionStringName}'.");
            }

            var migrationsAssembly = typeof(LuminaBoxDbContext).Assembly.GetName().Name;

            var options = new DbContextOptionsBuilder<LuminaBoxDbContext>()
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(migrationsAssembly))
                .Options;

            return new LuminaBoxDbContext(options);
        }

        /// <summary>
        /// Walks up from the build output folder and from the current directory until the
        /// API project folder is found, so the tools work from the solution root, the
        /// Infrastructure folder or the API folder.
        /// </summary>
        private static string ResolveApiDirectory()
        {
            var overrideDirectory = Environment.GetEnvironmentVariable(ApiDirectoryOverrideVariable);
            if (!string.IsNullOrWhiteSpace(overrideDirectory))
            {
                if (File.Exists(Path.Combine(overrideDirectory, "appsettings.json")))
                {
                    return Path.GetFullPath(overrideDirectory);
                }

                throw new DirectoryNotFoundException(
                    $"{ApiDirectoryOverrideVariable} points to '{overrideDirectory}', which has no appsettings.json.");
            }

            var startDirectories = new[]
            {
                AppContext.BaseDirectory,
                Path.GetDirectoryName(typeof(LuminaBoxDbContextFactory).Assembly.Location),
                Directory.GetCurrentDirectory()
            };

            foreach (var startDirectory in startDirectories.Where(d => !string.IsNullOrWhiteSpace(d)).Distinct())
            {
                for (var directory = new DirectoryInfo(startDirectory!); directory is not null; directory = directory.Parent)
                {
                    if (IsApiDirectory(directory.FullName))
                    {
                        return directory.FullName;
                    }

                    var sibling = Path.Combine(directory.FullName, ApiProjectName);
                    if (IsApiDirectory(sibling))
                    {
                        return sibling;
                    }
                }
            }

            throw new DirectoryNotFoundException(
                $"Could not locate the {ApiProjectName} project folder. " +
                $"Run the command inside the solution or set {ApiDirectoryOverrideVariable}.");
        }

        private static bool IsApiDirectory(string directory) =>
            File.Exists(Path.Combine(directory, $"{ApiProjectName}.csproj")) &&
            File.Exists(Path.Combine(directory, "appsettings.json"));

        private static string? ReadUserSecretsId(string apiDirectory)
        {
            var projectFile = Path.Combine(apiDirectory, $"{ApiProjectName}.csproj");

            return XDocument.Load(projectFile)
                .Descendants("UserSecretsId")
                .Select(e => e.Value.Trim())
                .FirstOrDefault(v => v.Length > 0);
        }
    }
}
