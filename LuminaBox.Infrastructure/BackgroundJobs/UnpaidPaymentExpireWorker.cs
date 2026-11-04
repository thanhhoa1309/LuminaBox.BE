using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LuminaBox.Application.Jobs.Payment;

namespace LuminaBox.Infrastructure.BackgroundJobs
{
    public class UnpaidPaymentExpireWorker : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UnpaidPaymentExpireWorker> _logger;

        public UnpaidPaymentExpireWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<UnpaidPaymentExpireWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var job = scope.ServiceProvider.GetRequiredService<UnpaidPaymentExpireJob>();
                    var expiredCount = await job.ExecuteAsync();
                    if (expiredCount > 0)
                    {
                        _logger.LogInformation("[UnpaidPaymentExpireWorker] Expired {Count} unpaid Stripe payments.", expiredCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[UnpaidPaymentExpireWorker] Failed while expiring unpaid payments.");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }
    }
}

