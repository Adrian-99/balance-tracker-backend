using API.Attributes;
using Application.Dtos;
using Application.Interfaces;
using Application.Mappers;
using Application.Utilities;
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

        [HttpGet]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<TagDto>>>> GetAllUnpaged()
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var tags = await tagService.GetAllAsync(user.Id);
            return Ok(ApiResponse<List<TagDto>>.Success(TagMapper.FromTagToTagDto(tags)));
        }

        [HttpPost]
        [Authorize(true)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<string>>> Create([FromBody] TagDto tagDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var tag = TagMapper.FromTagDtoToTag(user.Id, tagDto);
            await tagService.CreateAsync(tag);
            return Created("", ApiResponse<string>.Success("Tag successfully created", ""));
        }
    }
}
