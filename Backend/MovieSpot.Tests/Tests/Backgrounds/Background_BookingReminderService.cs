using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MovieSpot.Services.Background;
using MovieSpot.Services.Bookings;
using System.Net;

namespace MovieSpot.Tests.Services.Backgrounds
{
    public class Background_BookingReminderService
    {
        private readonly Mock<IServiceProvider> _mockRootProvider;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IServiceProvider> _mockScopedProvider;
        private readonly Mock<IBookingService> _mockBookingService;

        public Background_BookingReminderService()
        {
            _mockRootProvider = new Mock<IServiceProvider>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockScope = new Mock<IServiceScope>();
            _mockScopedProvider = new Mock<IServiceProvider>();
            _mockBookingService = new Mock<IBookingService>();
        }

        /// <summary>
        /// Helper to setup service provider and create the BookingReminderService instance.
        /// </summary>
        private BookingReminderService CreateService()
        {
            _mockScopedProvider
                .Setup(p => p.GetService(typeof(IBookingService)))
                .Returns(_mockBookingService.Object);

            _mockScope.Setup(s => s.ServiceProvider).Returns(_mockScopedProvider.Object);

            _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);

            _mockRootProvider
                .Setup(p => p.GetService(typeof(IServiceScopeFactory)))
                .Returns(_mockScopeFactory.Object);

            return new BookingReminderService(_mockRootProvider.Object);
        }

        [Fact]
        public async Task ExecuteAsync_CallsSendDailyRemindersAsync_WhenRunning()
        {
            var service = CreateService();

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(150);

            var runTask = service.StartAsync(cts.Token);
            await Task.Delay(100);
            cts.Cancel();
            await runTask;

            _mockBookingService.Verify(s => s.SendDailyRemindersAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_WhenSendThrows_PropagatesException()
        {
            _mockBookingService
                .Setup(s => s.SendDailyRemindersAsync())
                .ThrowsAsync(new InvalidOperationException("Fake reminder error"));

            var service = CreateService();

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(200);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartAsync(cts.Token));
        }

        [Fact]
        public async Task ExecuteAsync_WhenCancelledBeforeStart_DoesNotRunReminders()
        {
            var service = CreateService();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await service.StartAsync(cts.Token);

            _mockBookingService.Verify(s => s.SendDailyRemindersAsync(), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhenHttpRequestExceptionThrown_RethrowsHttpRequestException()
        {
            _mockBookingService
                .Setup(s => s.SendDailyRemindersAsync())
                .ThrowsAsync(new HttpRequestException("FCM network error"));

            var service = CreateService();

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(150);

            await Assert.ThrowsAsync<HttpRequestException>(() => service.StartAsync(cts.Token));
        }

        [Fact]
        public async Task ExecuteAsync_WhenDbUpdateExceptionThrown_RethrowsDbUpdateException()
        {
            _mockBookingService
                .Setup(s => s.SendDailyRemindersAsync())
                .ThrowsAsync(new DbUpdateException("Failed to save reminders to DB"));

            var service = CreateService();

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(150);

            await Assert.ThrowsAsync<DbUpdateException>(() => service.StartAsync(cts.Token));
        }

        [Fact]
        public async Task ExecuteAsync_WhenUnexpectedExceptionThrown_WrapsInCustomException()
        {
            var originalException = new ArgumentNullException("bookingService", "Unexpected null reference");
            _mockBookingService
                .Setup(s => s.SendDailyRemindersAsync())
                .ThrowsAsync(originalException);

            var service = CreateService();

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(200);

            var ex = await Assert.ThrowsAsync<Exception>(() => service.StartAsync(cts.Token));

            Assert.Contains("An unexpected error occurred during reminder processing.", ex.Message);
            Assert.NotNull(ex.InnerException);
            Assert.IsType<ArgumentNullException>(ex.InnerException);
            Assert.Contains("Unexpected null reference", ex.InnerException.Message);
        }
    }
}
