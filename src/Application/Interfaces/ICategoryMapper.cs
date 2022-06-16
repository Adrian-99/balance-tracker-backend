using Application.Dtos.Outgoing;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICategoryMapper
    {
        CategoryDto FromCategoryToCategoryDto(Category category);

        List<CategoryDto> FromCategoryToCategoryDto(List<Category> categories);
    }
}
