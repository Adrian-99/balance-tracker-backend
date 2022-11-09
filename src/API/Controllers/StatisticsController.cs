using API.Attributes;
using Application.Dtos.Ingoing.Statistics;
using Application.Dtos.Outgoing.Statistics;
using Application.Interfaces;
using Application.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    [Produces("application/json")]
    [ProducesErrorResponseType(typeof(ApiResponse<string>))]
    public class StatisticsController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IStatisticsService statisticsService;

        public StatisticsController(IUserService userService, IStatisticsService statisticsService)
        {
            this.userService = userService;
            this.statisticsService = statisticsService;
        }

        [HttpPost]
        [Authorize(true)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<StatisticsResponseDto>>> GenerateStatistics([FromBody] StatisticsRequestDto statisticsRequest)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var statisticsResponse = await statisticsService.GenerateStatisticsAsync(user.Id, statisticsRequest);
            return Ok(ApiResponse<StatisticsResponseDto>.Success(statisticsResponse));
        }
    }
}
