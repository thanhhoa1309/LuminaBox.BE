using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LuminaBox.Application.Interface;

namespace LuminaBox.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Cancels Stripe payments still Pending after 10 minutes (and their orders).
    /// </summary>
    public class UnpaidPaymentExpireWorker : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

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
                    var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                    var count = await paymentService.ExpireUnpaidPaymentsAsync();

                    if (count > 0)
                        _logger.LogInformation("Expired {Count} unpaid Stripe payment(s).", count);

                    var refunds = await paymentService.RetryPendingRefundsAsync();

                    if (refunds > 0)
                        _logger.LogInformation("Retried {Count} pending Stripe refund(s).", refunds);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Failed to expire unpaid Stripe payments.");
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}
