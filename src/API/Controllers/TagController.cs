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
    [Route("api/tag")]
    [ApiController]
    [Produces("application/json")]
    [ProducesErrorResponseType(typeof(ApiResponse<string>))]
    public class TagController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly ITagService tagService;

        public TagController(IUserService userService, ITagService tagService)
        {
            this.userService = userService;
            this.tagService = tagService;
        }

        [HttpGet("name")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetAllNames()
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var tags = await tagService.GetAllAsync(user.Id);
            return Ok(ApiResponse<List<string>>.Success(tags.Select(t => t.Name).ToList()));
        }

        [HttpGet]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Page<TagDto>>> GetAllPaged([FromQuery] Pageable pageable,
                                                                  [FromQuery] TagFilter tagFilter)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var tagsPage = await tagService.GetAllPagedAsync(user.Id, pageable, tagFilter);
            return Ok(tagsPage.Map(TagMapper.FromTagToTagDto));
        }

        [HttpPost]
        [Authorize(true)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<string>>> Create([FromBody] EditTagDto tagDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var tag = TagMapper.FromEditTagDtoToTag(user.Id, tagDto);
            await tagService.CreateAsync(tag);
            return Created("", ApiResponse<string>.Success("Tag successfully created", "success.tag.create"));
        }

        [HttpPut("{id}")]
        [Authorize(true)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<string>>> Edit([FromRoute] Guid id, [FromBody] EditTagDto tagDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var tag = TagMapper.FromEditTagDtoToTag(user.Id, tagDto);
            await tagService.UpdateAsync(id, tag);
            return Ok(ApiResponse<string>.Success("Tag successfully updated", "success.tag.edit"));
        }

        [HttpDelete("{id}")]
        [Authorize(true)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<string>>> Delete([FromRoute] Guid id, [FromQuery] string? replacementTags)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            await tagService.DeleteAsync(id, user.Id, replacementTags);
            return Ok(ApiResponse<string>.Success("Tag successfully deleted", "success.tag.delete"));
        }
    }
}
