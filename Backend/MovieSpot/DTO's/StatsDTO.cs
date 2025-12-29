namespace MovieSpot.DTO_s
{
    public class StatsDTO
    {
        public class StatsResponseDto
        {
            public int TotalSessions { get; set; }
            public int ActiveRooms { get; set; }
            public int TodaysSessions { get; set; }
            public int Movies { get; set; }
        }
    }
}
