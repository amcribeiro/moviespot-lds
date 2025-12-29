using static MovieSpot.DTO_s.StatsDTO;

namespace MovieSpot.Services.Stats
{
    public interface IStatsService
    {
        StatsResponseDto GetStats();
    }
}
