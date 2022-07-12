using API.Attributes;
using Application.Dtos.Outgoing;
using Application.Interfaces;
using Application.Mappers;
using Application.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/category")]
    [ApiController]
    [Produces("application/json")]
    [ProducesErrorResponseType(typeof(ApiResponse<string>))]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        [HttpGet]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll()
        {
            var categories = await categoryService.GetAllAsync();
            return Ok(ApiResponse<List<CategoryDto>>.Success(CategoryMapper.FromCategoryToCategoryDto(categories)));
        }
    }
}
