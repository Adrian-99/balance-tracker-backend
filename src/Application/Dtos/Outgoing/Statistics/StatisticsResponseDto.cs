using Application.Utilities.Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Outgoing.Statistics
{
    public class StatisticsResponseDto
    {
        [Required]
        public int EntriesCount { get; }

        [Required]
        public List<SelectValue> SelectValues { get; }

        public DateTime? DateFromFilter { get; }

        public DateTime? DateToFilter { get; }

        public EntryType? EntryTypeFilter { get; }

        public List<string>? CategoryFilter { get; }

        public List<string>? TagFilter { get; }

        [Required]
        public List<StatisticsRowDto> Rows { get; }

        public StatisticsResponseDto(int entriesCount,
                                    List<SelectValue> selectValues,
                                     DateTime? dateFromFilter,
                                     DateTime? dateToFilter,
                                     EntryType? entryTypeFilter,
                                     List<string>? categoryFilter,
                                     List<string>? tagFilter,
                                     List<StatisticsRowDto> rows)
        {
            EntriesCount = entriesCount;
            SelectValues = selectValues;
            DateFromFilter = dateFromFilter;
            DateToFilter = dateToFilter;
            EntryTypeFilter = entryTypeFilter;
            CategoryFilter = categoryFilter;
            TagFilter = tagFilter;
            Rows = rows;
        }
    }
}
