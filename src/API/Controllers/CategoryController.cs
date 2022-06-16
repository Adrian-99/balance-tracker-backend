using API.Attributes;
using Application.Dtos.Outgoing;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/category")]
    [ApiController]
    [Produces("application/json")]
    public class CategoryController : ControllerBase
    {
        private ICategoryService categoryService;
        private ICategoryMapper categoryMapper;

        public CategoryController(ICategoryService categoryService, ICategoryMapper categoryMapper)
        {
            this.categoryService = categoryService;
            this.categoryMapper = categoryMapper;
        }

        [HttpGet]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public async Task<List<CategoryDto>> GetAll()
        {
            var categories = await categoryService.GetAllAsync();
            return categoryMapper.FromCategoryToCategoryDto(categories);
        }
    }
}
