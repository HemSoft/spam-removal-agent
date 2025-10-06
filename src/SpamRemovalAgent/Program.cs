using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.AddConsole();

// Register hosted service (will be replaced with actual agent service in future features)
builder.Services.AddHostedService<HemSoft.SpamRemovalAgent.StartupService>();

var host = builder.Build();
await host.RunAsync();

namespace HemSoft.SpamRemovalAgent
{
    // Temporary startup service to satisfy FR-013
    public class StartupService : IHostedService
    {
        private readonly ILogger<StartupService> _logger;
        private readonly IHostApplicationLifetime _lifetime;

        public StartupService(ILogger<StartupService> logger, IHostApplicationLifetime lifetime)
        {
            _logger = logger;
            _lifetime = lifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Spam Removal Agent v1.0.0");

            // For scaffolding verification, exit immediately
            // In future features, this will be replaced with actual agent service
            _lifetime.StopApplication();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
