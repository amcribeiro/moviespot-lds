using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieSpot.Data;
using Xunit;

namespace MovieSpot.Tests.Integration.Config
{
    public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable

    {
        protected readonly HttpClient _client;
        protected readonly CustomWebApplicationFactory _factory;

        public IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
        protected ApplicationDbContext CreateDbContext()
        {
            var scope = _factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        public void Dispose()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureDeleted();
        }
    }
}
