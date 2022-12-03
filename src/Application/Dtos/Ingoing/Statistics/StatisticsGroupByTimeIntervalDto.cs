using Application.Utilities.Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing.Statistics
{
    public class StatisticsGroupByTimeIntervalDto
    {
        [Required]
        public DateTime ReferenceDate { get; }

        [Required]
        public int IntervalLength { get; }

        [Required]
        public TimePeriodUnit IntervalUnit { get; }

        public StatisticsGroupByTimeIntervalDto(DateTime referenceDate, int intervalLength, TimePeriodUnit intervalUnit)
        {
            ReferenceDate = referenceDate;
            IntervalLength = intervalLength;
            IntervalUnit = intervalUnit;
        }
    }
}
