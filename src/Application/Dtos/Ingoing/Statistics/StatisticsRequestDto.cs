using Application.Utilities.Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing.Statistics
{
    public class StatisticsRequestDto
    {
        public StatisticsDateRangeFilterDto? DateRangeFilter { get; }

        public EntryType? EntryTypeFilter { get; }

        public List<string>? CategoryFilter { get; }

        public List<string>? TagFilter { get; }

        public List<GroupBy>? GroupBy { get; }

        public StatisticsGroupByTimePeriodDto? GroupByTimePeriodProperties { get; }

        [Required]
        public List<SelectValue> SelectValues { get; }

        public bool? SelectOnAllLevels { get; }

        public StatisticsRequestDto(StatisticsDateRangeFilterDto? dateRangeFilter,
                                    EntryType? entryTypeFilter,
                                    List<string>? categoryFilter,
                                    List<string>? tagFilter,
                                    List<GroupBy>? groupBy,
                                    StatisticsGroupByTimePeriodDto? groupByTimePeriodProperties,
                                    List<SelectValue> selectValues,
                                    bool? selectOnAllLevels)
        {
            DateRangeFilter = dateRangeFilter;
            EntryTypeFilter = entryTypeFilter;
            CategoryFilter = categoryFilter;
            TagFilter = tagFilter;
            GroupBy = groupBy;
            GroupByTimePeriodProperties = groupByTimePeriodProperties;
            SelectValues = selectValues;
            SelectOnAllLevels = selectOnAllLevels;
        }
    }
}
