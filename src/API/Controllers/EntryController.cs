using API.Attributes;
using Application.Dtos.Outgoing;
using Application.Interfaces;
using Application.Mappers;
using Application.Utilities;
using Application.Utilities.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/entry")]
    [ApiController]
    [Produces("application/json")]
    [ProducesErrorResponseType(typeof(ApiResponse<string>))]
    public class EntryController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IEntryService entryService;

        public EntryController(IUserService userService, IEntryService entryService)
        {
            this.userService = userService;
            this.entryService = entryService;
        }

        [HttpGet]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Page<EntryDto>>> GetAllPaged([FromQuery] Pageable pageable,
                                                                    [FromQuery] EntryFilter entryFilter)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var entriesPage = await entryService.GetAllPagedAsync(user.Id, pageable, entryFilter);
            return Ok(entriesPage.Map(EntryMapper.FromEntryToEntryDto));
        }
    }
}
