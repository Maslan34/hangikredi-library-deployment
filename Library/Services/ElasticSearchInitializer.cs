using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Services
{
    public class ElasticsearchInitializer : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ElasticsearchInitializer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var bookIndexService = scope.ServiceProvider.GetRequiredService<BookIndexService>();

            await bookIndexService.InitializeAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
