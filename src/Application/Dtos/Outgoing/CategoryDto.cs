using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Outgoing
{
    public class CategoryDto
    {
        [Required]
        public Guid Id { get; }

        [Required]
        public string Keyword { get; }

        [Required]
        public bool IsIncome { get; }

        [Required]
        public string Icon { get; }

        [Required]
        public string IconColor { get; }

        public CategoryDto(Guid id, string keyword, bool isIncome, string icon, string iconColor)
        {
            Id = id;
            Keyword = keyword;
            IsIncome = isIncome;
            Icon = icon;
            IconColor = iconColor;
        }
    }
}
