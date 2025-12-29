using MovieSpot.Services.Bookings;

namespace MovieSpot.Services.Background
{
    /// <summary>
    /// Background service responsável por limpar reservas expiradas
    /// e libertar lugares automaticamente em intervalos regulares.
    /// </summary>
    public class BookingExpirationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        // ⏱️ por ex: corre a cada 1 minuto
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

        public BookingExpirationBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _serviceProvider.CreateAsyncScope();

                    var expirationService = scope.ServiceProvider
                        .GetRequiredService<IBookingService>();

                    await expirationService.CleanupExpiredBookingsAsync();
                }
                catch (Exception)
                {
                    // aqui em vez de dar throw, convém logar só:
                    // _logger.LogError(ex, "Erro ao limpar reservas expiradas.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
