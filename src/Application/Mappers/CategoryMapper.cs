using Application.Dtos.Outgoing;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers
{
    public class CategoryMapper
    {
        public static CategoryDto FromCategoryToCategoryDto(Category category)
        {
            return new CategoryDto(
                category.Id,
                category.Keyword,
                category.IsIncome,
                category.Icon,
                category.IconColor
                );
        }

        public static List<CategoryDto> FromCategoryToCategoryDto(List<Category> categories)
        {
            return categories.Select(category => FromCategoryToCategoryDto(category)).ToList();
        }
    }
}
