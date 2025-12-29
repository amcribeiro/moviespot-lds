using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using MovieSpot.Models;
using MovieSpot.Services.Tmdb;
using Xunit;

namespace MovieSpot.Tests.Integration.Services
{
    /// <summary>
    /// Integration tests for <see cref="TmdbApiService"/>.
    /// These tests verify the integration between the service logic
    /// and the external TMDB API through an HttpClient mock.
    /// </summary>
    public class TMDBAPIServiceTest
    {
        #region Helper: Mock HttpMessageHandler

        private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, object? responseBody)
        {
            var handler = new MockHttpMessageHandler(statusCode, responseBody);
            return new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.themoviedb.org/3/")
            };
        }

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly object? _responseBody;

            public MockHttpMessageHandler(HttpStatusCode statusCode, object? responseBody)
            {
                _statusCode = statusCode;
                _responseBody = responseBody;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(_statusCode);

                if (_responseBody != null)
                {
                    var json = JsonSerializer.Serialize(_responseBody);
                    response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                }
                else
                {
                    response.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
                }

                return Task.FromResult(response);
            }
        }

        #endregion

        #region GetTrendingMovies

        [Fact]
        public async Task GetTrendingMovies_ReturnsListOfIds_WhenResponseIsValid()
        {
            var movies = new List<MovieFromAPI>
            {
                new() { Id = 101, Title = "Test Movie 1" },
                new() { Id = 202, Title = "Test Movie 2" }
            };

            var response = new
            {
                results = movies
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, response);
            var service = new TmdbApiService(httpClient);

            var result = await service.GetTrendingMovies("day");

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(101, result);
            Assert.Contains(202, result);
        }

        [Fact]
        public async Task GetTrendingMovies_ReturnsEmptyList_WhenResponseHasNoResults()
        {
            var response = new { results = (List<MovieFromAPI>?)null };
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, response);
            var service = new TmdbApiService(httpClient);

            var result = await service.GetTrendingMovies("week");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTrendingMovies_UsesDefaultTimeWindow_WhenParameterIsNullOrEmpty()
        {
            var response = new { results = new List<MovieFromAPI>() };
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, response);
            var service = new TmdbApiService(httpClient);

            var result = await service.GetTrendingMovies(null!);

            Assert.Empty(result);
        }

        #endregion

        #region GetMovieFromAPI

        [Fact]
        public async Task GetMovieFromAPI_ReturnsMovie_WhenResponseIsValid()
        {
            var movie = new MovieFromAPI
            {
                Id = 321,
                Title = "Inception",
                Overview = "A dream within a dream",
                Runtime = 148,
                OriginalLanguage = "en",
                ReleaseDate = "2010-07-16",
                PosterPath = "/poster.jpg",
                OriginCountry = new List<string> { "US" },
                Genres = new List<Genre> { new Genre { Id = 1, Name = "Sci-Fi" } }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, movie);
            var service = new TmdbApiService(httpClient);

            var result = await service.GetMovieFromAPI(321);

            Assert.NotNull(result);
            Assert.Equal("Inception", result!.Title);
            Assert.Equal(148, result.Runtime);
            Assert.Single(result.Genres);
            Assert.Equal("Sci-Fi", result.Genres.First().Name);
        }
        [Fact]
        public async Task GetMovieFromAPI_ReturnsEmptyMovie_WhenResponseIsEmpty()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.NoContent, null);
            var service = new TmdbApiService(httpClient);

            var result = await service.GetMovieFromAPI(999);

            Assert.NotNull(result);
            Assert.Equal(0, result!.Id);
            Assert.Equal(string.Empty, result.Title);
            Assert.Equal(string.Empty, result.Overview);
        }


        [Fact]
        public async Task GetMovieFromAPI_NormalizesNullFields()
        {
            var movie = new MovieFromAPI
            {
                Id = 55,
                Title = null!,
                Overview = null!,
                OriginalLanguage = null!,
                ReleaseDate = null!,
                PosterPath = null!,
                OriginCountry = null!,
                Genres = null!
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, movie);
            var service = new TmdbApiService(httpClient);

            var result = await service.GetMovieFromAPI(55);

            Assert.NotNull(result);
            Assert.Equal(string.Empty, result!.Title);
            Assert.Empty(result.OriginCountry);
            Assert.Empty(result.Genres);
        }

        #endregion

        #region GetTrendingMoviesWithDetails

        [Fact]
        public async Task GetTrendingMoviesWithDetails_ReturnsDetailedMovies()
        {

            var trendingResponse = new
            {
                results = new List<MovieFromAPI>
                {
                    new() { Id = 1 },
                    new() { Id = 2 }
                }
            };

            var movie1 = new MovieFromAPI { Id = 1, Title = "Movie One" };
            var movie2 = new MovieFromAPI { Id = 2, Title = "Movie Two" };

            var handler = new SequenceMockHandler(new (HttpStatusCode, object?)[]
            {
                (HttpStatusCode.OK, trendingResponse),
                (HttpStatusCode.OK, movie1),
                (HttpStatusCode.OK, movie2)
            });

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.themoviedb.org/3/")
            };
            var service = new TmdbApiService(httpClient);

            var result = await service.GetTrendingMoviesWithDetails("day");

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Title == "Movie One");
            Assert.Contains(result, m => m.Title == "Movie Two");
        }

        private class SequenceMockHandler : HttpMessageHandler
        {
            private readonly Queue<(HttpStatusCode, object?)> _responses;

            public SequenceMockHandler(IEnumerable<(HttpStatusCode, object?)> responses)
            {
                _responses = new Queue<(HttpStatusCode, object?)>(responses);
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (_responses.Count == 0)
                    throw new InvalidOperationException("No more responses in queue.");

                var (status, body) = _responses.Dequeue();
                var response = new HttpResponseMessage(status);

                if (body != null)
                {
                    var json = JsonSerializer.Serialize(body);
                    response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                }

                return Task.FromResult(response);
            }
        }

        #endregion
    }
}
