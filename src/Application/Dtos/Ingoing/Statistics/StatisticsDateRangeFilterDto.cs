using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing.Statistics
{
    public class StatisticsDateRangeFilterDto
    {
        [Required]
        public DateTime DateFrom { get; }

        [Required]
        public DateTime DateTo { get; }

        public StatisticsDateRangeFilterDto(DateTime dateFrom, DateTime dateTo)
        {
            DateFrom = dateFrom;
            DateTo = dateTo;
        }
    }
}
