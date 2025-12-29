using MovieSpot.Services.Genres;

namespace MovieSpot.Services.Background
{
    /// <summary>
    /// Background service responsible for automatically synchronizing
    /// movie genres from the TMDB API with the local database at a fixed interval.
    /// </summary>
    public class GenreSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromDays(7);

        /// <summary>
        /// Initializes a new instance of the <see cref="GenreSyncService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve scoped dependencies.</param>
        public GenreSyncService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Executes the background synchronization task repeatedly while the application is running.
        /// </summary>
        /// <param name="stoppingToken">
        /// A cancellation token that signals when the background task should stop.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the <see cref="IGenreService"/> cannot be resolved from the service provider.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// Thrown when there is an issue communicating with the TMDB API.
        /// </exception>
        /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">
        /// Thrown when saving synchronized genres to the local database fails.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when any unexpected error occurs during genre synchronization.
        /// </exception>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var genreService = scope.ServiceProvider.GetRequiredService<IGenreService>();

                    await genreService.SyncGenresAsync();
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
                    throw new Exception("An unexpected error occurred during genre synchronization.", ex);
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
