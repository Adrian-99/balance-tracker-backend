using API.Attributes;
using Application.Dtos.Ingoing;
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
        public async Task<ActionResult<ApiResponse<string>>> Create([FromBody] EditEntryDto editEntryDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            entryService.ValidateDescription(editEntryDto.Description);
            var entry = await entryMapper.FromEditEntryDtoToEntryAsync(editEntryDto, user.Id);
            await entryService.CreateAsync(entry, editEntryDto.TagNames);
            return Created("", ApiResponse<string>.Success("Entry successfully created"));
        }

        [HttpPut("{id}")]
        [Authorize(true)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<string>>> Edit([FromRoute] Guid id, [FromBody] EditEntryDto editEntryDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            await entryService.AssertEntryExistsAsync(id, user.Id);
            entryService.ValidateDescription(editEntryDto.Description);
            var entry = await entryMapper.FromEditEntryDtoToEntryAsync(editEntryDto, user.Id);
            await entryService.UpdateAsync(id, entry, editEntryDto.TagNames);
            return Ok(ApiResponse<string>.Success("Entry successfully updated"));
        }

        [HttpDelete("{id}")]
        [Authorize(true)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<string>>> Delete([FromRoute] Guid id)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            await entryService.DeleteAsync(id, user.Id);
            return Ok(ApiResponse<string>.Success("Entry successfully deleted"));
        }
    }
}
