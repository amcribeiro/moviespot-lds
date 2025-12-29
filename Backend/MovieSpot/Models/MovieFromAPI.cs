using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MovieSpot.Models
{
    /// <summary>
    /// Represents a movie object retrieved from the external TMDB API.
    /// </summary>
    public class MovieFromAPI
    {
        /// <summary>
        /// The unique identifier of the movie provided by the TMDB API.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// The official title of the movie.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// A brief overview or description of the movie plot.
        /// </summary>
        [JsonPropertyName("overview")]
        public string Overview { get; set; } = string.Empty;

        /// <summary>
        /// The duration of the movie in minutes.
        /// </summary>
        [JsonPropertyName("runtime")]
        public int Runtime { get; set; }

        /// <summary>
        /// The original language in which the movie was produced (ISO 639-1 code).
        /// </summary>
        [JsonPropertyName("original_language")]
        public string OriginalLanguage { get; set; } = string.Empty;

        /// <summary>
        /// The release date of the movie in ISO 8601 format (yyyy-MM-dd).
        /// </summary>
        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        /// <summary>
        /// The list of countries where the movie was originally produced.
        /// </summary>
        [JsonPropertyName("origin_country")]
        public List<string> OriginCountry { get; set; } = new();

        /// <summary>
        /// The relative path to the poster image (combine with TMDB base image URL).
        /// </summary>
        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; } = string.Empty;

        /// <summary>
        /// The list of genre identifiers associated with the movie.
        /// </summary>
        [JsonPropertyName("genres")]
        public List<Genre> Genres { get; set; } = new();
    }
}

