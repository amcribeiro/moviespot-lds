using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MovieSpot.Services.Notifications;

namespace MovieSpot.Tests.Integration.Config
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IFcmNotificationService> FcmMock { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IFcmNotificationService));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddSingleton(FcmMock.Object);

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = FakeAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = FakeAuthHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>(
                    FakeAuthHandler.AuthenticationScheme,
                    _ => { });
            });
        }
    }
}