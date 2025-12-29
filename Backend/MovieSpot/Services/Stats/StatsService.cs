using MovieSpot.Data;
using System;
using System.Linq;
using static MovieSpot.DTO_s.StatsDTO;

namespace MovieSpot.Services.Stats
{
    public class StatsService : IStatsService
    {
        private readonly ApplicationDbContext _context;

        public StatsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public StatsResponseDto GetStats()
        {
            var today = DateTime.UtcNow.Date;
            var now = DateTime.UtcNow;

            var sessions = _context.Session.ToList();
            var moviesCount = _context.Movie.Count();

            var totalSessions = sessions.Count;

            var todaysSessions = sessions.Count(s =>
                s.StartDate.Date == today
            );

            var activeRooms = sessions
                .Where(s => s.StartDate >= now)
                .Select(s => s.CinemaHallId)
                .Distinct()
                .Count();

            return new StatsResponseDto
            {
                TotalSessions = totalSessions,
                TodaysSessions = todaysSessions,
                ActiveRooms = activeRooms,
                Movies = moviesCount
            };
        }
    }
}
