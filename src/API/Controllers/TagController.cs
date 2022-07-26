using API.Attributes;
using Application.Interfaces;
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
        public async Task<ActionResult<ApiResponse<List<string>>>> GetAllUnpaged()
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var tags = await tagService.GetAll(user.Id);
            return Ok(ApiResponse<List<string>>.Success(tags.Select(tag => tag.Name).ToList()));
        }
    }
}
