using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MovieSpot.Services.Background;
using MovieSpot.Services.Genres;

namespace MovieSpot.Tests.Services.Backgrounds
{
    public class Background_GenreServiceTest
    {
        private readonly Mock<IServiceProvider> _mockRootProvider;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IServiceProvider> _mockScopedProvider;
        private readonly Mock<IGenreService> _mockGenreService;

        public Background_GenreServiceTest()
        {
            _mockRootProvider = new Mock<IServiceProvider>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockScope = new Mock<IServiceScope>();
            _mockScopedProvider = new Mock<IServiceProvider>();
            _mockGenreService = new Mock<IGenreService>();
        }

        /// <summary>
        /// Helper that sets up the service provider and creates the background service.
        /// </summary>
        private GenreSyncService CreateService()
        {
            _mockScopedProvider
                .Setup(p => p.GetService(typeof(IGenreService)))
                .Returns(_mockGenreService.Object);

            _mockScope.Setup(s => s.ServiceProvider).Returns(_mockScopedProvider.Object);

            _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);

            _mockRootProvider
                .Setup(p => p.GetService(typeof(IServiceScopeFactory)))
                .Returns(_mockScopeFactory.Object);

            return new GenreSyncService(_mockRootProvider.Object);
        }

        [Fact]
        public async Task ExecuteAsync_CallsSyncGenresAsync_WhenRunning()
        {
            var service = CreateService();

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(150);

            var runTask = service.StartAsync(cts.Token);
            await Task.Delay(100);
            cts.Cancel();
            await runTask;

            _mockGenreService.Verify(s => s.SyncGenresAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_WhenSyncThrows_PropagatesException()
        {
            _mockGenreService
                .Setup(s => s.SyncGenresAsync())
                .ThrowsAsync(new InvalidOperationException("Fake sync error"));

            var service = CreateService();

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(200);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartAsync(cts.Token));
        }

        [Fact]
        public async Task ExecuteAsync_WhenCancelledBeforeStart_DoesNotRunSync()
        {
            var service = CreateService();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await service.StartAsync(cts.Token);

            _mockGenreService.Verify(s => s.SyncGenresAsync(), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhenHttpRequestExceptionThrown_RethrowsHttpRequestException()
        {
            _mockGenreService
                .Setup(s => s.SyncGenresAsync())
                .ThrowsAsync(new HttpRequestException("TMDB network error"));

            var service = CreateService();

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(150);

            await Assert.ThrowsAsync<HttpRequestException>(() => service.StartAsync(cts.Token));
        }

        [Fact]
        public async Task ExecuteAsync_WhenDbUpdateExceptionThrown_RethrowsDbUpdateException()
        {
            _mockGenreService
                .Setup(s => s.SyncGenresAsync())
                .ThrowsAsync(new DbUpdateException("Failed to save genres to DB"));

            var service = CreateService();

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(150);

            await Assert.ThrowsAsync<DbUpdateException>(() => service.StartAsync(cts.Token));
        }

        [Fact]
        public async Task ExecuteAsync_WhenUnexpectedExceptionThrown_WrapsInCustomException()
        {
            var originalException = new ArgumentNullException("genreService", "Unexpected null reference");
            _mockGenreService
                .Setup(s => s.SyncGenresAsync())
                .ThrowsAsync(originalException);

            var service = CreateService();

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(200);

            var ex = await Assert.ThrowsAsync<Exception>(() => service.StartAsync(cts.Token));

            Assert.Contains("An unexpected error occurred during genre synchronization.", ex.Message);
            Assert.NotNull(ex.InnerException);
            Assert.IsType<ArgumentNullException>(ex.InnerException);
            Assert.Contains("Unexpected null reference", ex.InnerException.Message);
        }
    }
}