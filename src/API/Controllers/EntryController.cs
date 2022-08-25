using API.Attributes;
using Application.Dtos;
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
        private readonly IEntryMapper entryMapper;

        public EntryController(IUserService userService, IEntryService entryService, IEntryMapper entryMapper)
        {
            this.userService = userService;
            this.entryService = entryService;
            this.entryMapper = entryMapper;
        }

        [HttpGet]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Page<EntryDto>>> GetAllPaged([FromQuery] Pageable pageable,
                                                                    [FromQuery] EntryFilter entryFilter)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var entriesPage = await entryService.GetAllPagedAsync(user.Id, pageable, entryFilter);
            return Ok(entriesPage.Map(entryMapper.FromEntryToEntryDto));
        }

        [HttpPost]
        [Authorize(true)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<string>>> Create([FromBody] EntryDto entryDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var entry = await entryMapper.FromEntryDtoToEntryAsync(entryDto, user.Id);
            await entryService.CreateAsync(entry, entryDto.Tags.Select(t => t.Name).ToList());
            return Created("", ApiResponse<string>.Success("Entry successfully created", ""));
        }
    }
}
