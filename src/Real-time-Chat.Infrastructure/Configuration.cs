using Microsoft.Extensions.Configuration;

namespace Real_time_Chat.Infrastructure;

static class Configuration
{
    static public string ConnectionString
    {
        get
        {
            var configurationManager = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + "/../Real-time-Chat.WebApi")
                // .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return configurationManager.GetConnectionString("PostgreSQL");
        }
    }
}
