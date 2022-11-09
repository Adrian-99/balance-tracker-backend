using Application.Utilities.Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Outgoing.Statistics
{
    public class StatisticsRowDto
    {
        public DateTime? DateFrom { get; }

        public DateTime? DateTo { get; }

        public EntryType? EntryType { get; }

        public string? CategoryKeyword { get; }

        public string? TagName { get; }

        public Dictionary<SelectValue, decimal>? Values { get; }

        public List<StatisticsRowDto>? SubRows { get; }

        public StatisticsRowDto(DateTime? dateFrom,
                                DateTime? dateTo,
                                EntryType? entryType,
                                string? categoryKeyword,
                                string? tagName,
                                Dictionary<SelectValue, decimal>? values,
                                List<StatisticsRowDto>? subRows)
        {
            DateFrom = dateFrom;
            DateTo = dateTo;
            EntryType = entryType;
            CategoryKeyword = categoryKeyword;
            TagName = tagName;
            Values = values;
            SubRows = subRows;
        }
    }
}
