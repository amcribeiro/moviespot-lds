using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using MovieSpot.Models;
using MovieSpot.Services.Tmdb;
using Xunit;

namespace MovieSpot.Tests.Services.Movies
{
    public class TMDBAPIServiceTest
    {
        #region Helper - Mock HttpMessageHandler

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly object? _response;

            public MockHttpMessageHandler(HttpStatusCode statusCode, object? response)
            {
                _statusCode = statusCode;
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var message = new HttpResponseMessage(_statusCode);

                if (_response != null)
                {
                    var json = JsonSerializer.Serialize(_response);
                    message.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                }
                else
                {
                    message.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
                }

                return Task.FromResult(message);
            }
        }

        private static TmdbApiService CreateService(HttpStatusCode status, object? response)
        {
            var handler = new MockHttpMessageHandler(status, response);
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.themoviedb.org/3/")
            };

            return new TmdbApiService(client);
        }

        #endregion

        #region GetTrendingMovies

        [Fact]
        public async Task GetTrendingMovies_WhenResponseValid_ReturnsIds()
        {
            var response = new
            {
                results = new List<MovieFromAPI>
                {
                    new() { Id = 1, Title = "Movie A" },
                    new() { Id = 2, Title = "Movie B" }
                }
            };

            var service = CreateService(HttpStatusCode.OK, response);

            var result = await service.GetTrendingMovies("day");

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(1, result);
            Assert.Contains(2, result);
        }

        [Fact]
        public async Task GetTrendingMovies_WhenResponseHasNoResults_ReturnsEmpty()
        {
            var response = new { results = (List<MovieFromAPI>?)null };
            var service = CreateService(HttpStatusCode.OK, response);

            var result = await service.GetTrendingMovies("week");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTrendingMovies_WhenTimeWindowIsNull_UsesDefaultWeek()
        {
            var response = new { results = new List<MovieFromAPI>() };
            var service = CreateService(HttpStatusCode.OK, response);

            var result = await service.GetTrendingMovies(null!);

            Assert.Empty(result);
        }

        #endregion

        #region GetMovieFromAPI

        [Fact]
        public async Task GetMovieFromAPI_WhenValid_ReturnsMovie()
        {
            var movie = new MovieFromAPI
            {
                Id = 10,
                Title = "Inception",
                Overview = "Dream within a dream",
                OriginalLanguage = "en",
                ReleaseDate = "2010-07-16",
                PosterPath = "/poster.jpg",
                Genres = new List<Genre> { new Genre { Id = 1, Name = "Sci-Fi" } },
                OriginCountry = new List<string> { "US" }
            };

            var service = CreateService(HttpStatusCode.OK, movie);

            var result = await service.GetMovieFromAPI(10);

            Assert.NotNull(result);
            Assert.Equal("Inception", result!.Title);
            Assert.Single(result.Genres);
            Assert.Contains("US", result.OriginCountry);
        }

        [Fact]
        public async Task GetMovieFromAPI_WhenResponseEmpty_ReturnsEmptyMovie()
        {
            var service = CreateService(HttpStatusCode.OK, new { });

            var result = await service.GetMovieFromAPI(999);

            Assert.NotNull(result);
            Assert.Equal(0, result!.Id);
            Assert.Equal(string.Empty, result.Title);
            Assert.NotNull(result.Genres);
        }

        [Fact]
        public async Task GetMovieFromAPI_WhenNullFields_NormalizesValues()
        {
            var movie = new MovieFromAPI
            {
                Id = 15,
                Title = null!,
                Overview = null!,
                OriginalLanguage = null!,
                ReleaseDate = null!,
                PosterPath = null!,
                Genres = null!,
                OriginCountry = null!
            };

            var service = CreateService(HttpStatusCode.OK, movie);

            var result = await service.GetMovieFromAPI(15);

            Assert.NotNull(result);
            Assert.Equal(string.Empty, result!.Title);
            Assert.Empty(result.Genres);
            Assert.Empty(result.OriginCountry);
        }

        #endregion

        #region GetTrendingMoviesWithDetails

        [Fact]
        public async Task GetTrendingMoviesWithDetails_ReturnsMoviesWithDetails()
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

            var sequenceHandler = new Queue<(HttpStatusCode, object?)>(new (HttpStatusCode, object?)[]
            {
                (HttpStatusCode.OK, trendingResponse),
                (HttpStatusCode.OK, movie1),
                (HttpStatusCode.OK, movie2)
            });

            var handler = new CustomSequenceHandler(sequenceHandler);
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.themoviedb.org/3/")
            };

            var service = new TmdbApiService(client);

            var result = await service.GetTrendingMoviesWithDetails("day");

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Title == "Movie One");
            Assert.Contains(result, m => m.Title == "Movie Two");
        }

        private class CustomSequenceHandler : HttpMessageHandler
        {
            private readonly Queue<(HttpStatusCode, object?)> _responses;

            public CustomSequenceHandler(Queue<(HttpStatusCode, object?)> responses)
            {
                _responses = responses;
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
