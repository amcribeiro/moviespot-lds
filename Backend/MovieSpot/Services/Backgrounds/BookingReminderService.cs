using MovieSpot.Services.Bookings;
using MovieSpot.Services.Notifications;

namespace MovieSpot.Services.Background
{
    /// <summary>
    /// Background service responsible for automatically sending daily reminders
    /// to users with bookings for the following day.
    /// </summary>
    public class BookingReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly TimeSpan _interval = TimeSpan.FromDays(7);

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingReminderService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve scoped dependencies.</param>
        public BookingReminderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Executes the background reminder task repeatedly while the application is running.
        /// </summary>
        /// <param name="stoppingToken">
        /// A cancellation token that signals when the background task should stop.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the <see cref="BookingService"/> cannot be resolved from the service provider.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// Thrown when there is an issue communicating with the external service.
        /// </exception>
        /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">
        /// Thrown when saving reminders to the local database fails.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when any unexpected error occurs during execution.
        /// </exception>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    await bookingService.SendDailyRemindersAsync();
                }
                catch (HttpRequestException)
                {
                    throw;
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException)
                {
                    throw;
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new Exception("An unexpected error occurred during reminder processing.", ex);
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
