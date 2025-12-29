using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieSpot.Models
{
    public class Genre
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        [JsonProperty("name")]
        public required string Name { get; set; }
        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();

    }

    public class GenreStatDto
    {
        public string GenreName { get; set; } = string.Empty;
        public int MoviesCount { get; set; }
        public int SessionsCount { get; set; }
}
    }
