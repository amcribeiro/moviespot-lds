using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieSpot.Services.Stats;
using static MovieSpot.DTO_s.StatsDTO;

namespace MovieSpot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _statsService;
        private readonly ILogger<StatsController> _logger;

        public StatsController(IStatsService statsService, ILogger<StatsController> logger)
        {
            _statsService = statsService;
            _logger = logger;
        }

        /// <summary>
        /// Returns aggregated stats for the dashboard.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(StatsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<StatsResponseDto> GetStats()
        {
            try
            {
                var stats = _statsService.GetStats();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calculating stats.");
                return BadRequest(new { message = "Failed to retrieve stats." });
            }
        }
    }
}
