using Application.Utilities.Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing.Statistics
{
    public class StatisticsGroupByTimePeriodDto
    {
        [Required]
        public DateTime StartDate { get; }

        [Required]
        public int IntervalValue { get; }

        [Required]
        public TimePeriodUnit IntervalUnit { get; }

        public StatisticsGroupByTimePeriodDto(DateTime startDate, int intervalValue, TimePeriodUnit intervalUnit)
        {
            StartDate = startDate;
            IntervalValue = intervalValue;
            IntervalUnit = intervalUnit;
        }
    }
}
