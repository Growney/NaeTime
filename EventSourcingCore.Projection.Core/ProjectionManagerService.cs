using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventSourcingCore.Projection.Core
{
    internal class ProjectionManagerService : IHostedService
    {
        private readonly ILogger<ProjectionManagerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        public ProjectionManagerService(ILogger<ProjectionManagerService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting projection managers");
            var managers = _serviceProvider.GetServices<IProjectionManager>();
            foreach (var manager in managers)
            {
                await manager.Start();
            }
        }
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
