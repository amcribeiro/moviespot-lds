namespace MovieSpot.DTO_s
{
    /// <summary>
    /// Lightweight representation of a movie including its genres.
    /// Used to avoid circular references when serializing.
    /// </summary>
    public class MovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public string Country { get; set; } = string.Empty;
        public string PosterPath { get; set; } = string.Empty;
        public int Duration { get; set; }
        public List<string> Genres { get; set; } = new();
    }

    /// <summary>
    /// Simplified view of a movie retrieved from the TMDB API.
    /// </summary>
    public class MovieFromApiDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string PosterPath { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
    }
}
