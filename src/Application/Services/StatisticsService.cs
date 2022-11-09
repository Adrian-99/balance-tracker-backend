using Application.Dtos.Ingoing.Statistics;
using Application.Dtos.Outgoing.Statistics;
using Application.Exceptions;
using Application.Interfaces;
using Application.Utilities;
using Application.Utilities.Statistics;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    internal class StatisticsService : IStatisticsService
    {
        private readonly IEntryRepository entryRepository;

        public StatisticsService(IEntryRepository entryRepository)
        {
            this.entryRepository = entryRepository;
        }

        public async Task<StatisticsResponseDto> GenerateStatisticsAsync(Guid userId, StatisticsRequestDto statisticsRequest)
        {
            ValidateStatisticsRequest(statisticsRequest);
            var entries = await entryRepository.GetAllByUserIdAsync(userId);
            var filteredEntries = FilterEntries(entries, statisticsRequest);
            var statisticsTopLevelRows = GroupAndSelect(filteredEntries, statisticsRequest, 0, statisticsRequest.EntryTypeFilter != null);
            return new StatisticsResponseDto(
                filteredEntries.Count,
                statisticsRequest.DateRangeFilter != null ? statisticsRequest.DateRangeFilter.DateFrom.Date : null,
                statisticsRequest.DateRangeFilter != null ? statisticsRequest.DateRangeFilter.DateTo.Date.AddDays(1).AddTicks(-1) : null,
                statisticsRequest.EntryTypeFilter,
                statisticsRequest.CategoryFilter,
                statisticsRequest.TagFilter,
                statisticsTopLevelRows
                );
        }

        private void ValidateStatisticsRequest(StatisticsRequestDto statisticsRequest)
        {
            if (statisticsRequest.SelectValues.Count < 1)
            {
                throw new DataValidationException("At least one element for selectValues is required");
            }
            else if (statisticsRequest.DateRangeFilter != null && statisticsRequest.DateRangeFilter.DateFrom.CompareTo(statisticsRequest.DateRangeFilter.DateTo) > 0)
            {
                throw new DataValidationException("Incorrect date range - dateRangeFilter.dateTo must not be before dateRangeFilter.dateFrom");
            }
            else if (statisticsRequest.GroupBy != null && statisticsRequest.GroupBy.Count > statisticsRequest.GroupBy.Distinct().Count())
            {
                throw new DataValidationException("Found duplicate values in groupBy");
            }
            else if (statisticsRequest.GroupBy != null && statisticsRequest.GroupBy.Contains(GroupBy.TimePeriod) && statisticsRequest.GroupByTimePeriodProperties == null)
            {
                throw new DataValidationException("Missing required groupByTimePeriodProperties while groupBy contains timePeriod");
            }
            else if (statisticsRequest.GroupByTimePeriodProperties != null && statisticsRequest.GroupByTimePeriodProperties.IntervalValue < 1)
            {
                throw new DataValidationException("Incorrect value of groupByTimePeriodProperties.intervalValue - must not be lower than 1");
            }
            else if (statisticsRequest.SelectValues.Count > statisticsRequest.SelectValues.Distinct().Count())
            {
                throw new DataValidationException("Found duplicate values in selectValues");
            }
        }

        private List<Entry> FilterEntries(List<Entry> entries, StatisticsRequestDto statisticsRequest)
        {
            if (statisticsRequest.DateRangeFilter != null)
            {
                var dateFrom = statisticsRequest.DateRangeFilter.DateFrom.Date;
                var dateTo = statisticsRequest.DateRangeFilter.DateTo.Date.AddDays(1).AddTicks(-1);
                entries = entries.Where(
                    entry => entry.Date.CompareTo(dateFrom) >= 0 &&
                        entry.Date.CompareTo(dateTo) <= 0
                    ).ToList();
            }
            if (statisticsRequest.EntryTypeFilter != null)
            {
                entries = entries.Where(
                    entry => statisticsRequest.EntryTypeFilter == EntryType.Income ? entry.Category.IsIncome : !entry.Category.IsIncome
                    ).ToList();
            }
            if (statisticsRequest.CategoryFilter != null)
            {
                entries = entries.Where(
                    entry => Utils.EqualsAnyIgnoreCase(entry.Category.Keyword, statisticsRequest.CategoryFilter.ToArray())
                    ).ToList();
            }
            if (statisticsRequest.TagFilter != null)
            {
                entries = entries.Where(
                    entry => entry.EntryTags.Any(entryTag => Utils.EqualsAnyIgnoreCase(entryTag.Tag.Name, statisticsRequest.TagFilter.ToArray()))
                    ).ToList();
            }
            return entries;
        }

        private List<StatisticsRowDto> GroupAndSelect(List<Entry> entries,
                                                      StatisticsRequestDto statisticsRequest,
                                                      int groupByLevel,
                                                      bool dontNegateCostValues)
        {
            if (statisticsRequest.GroupBy != null && groupByLevel < statisticsRequest.GroupBy.Count)
            {
                if (entries.Count > 0)
                {
                    return statisticsRequest.GroupBy[groupByLevel] switch
                    {
                        GroupBy.TimePeriod => GroupByTimePeriodAndSelect(entries, statisticsRequest, groupByLevel, dontNegateCostValues),
                        GroupBy.EntryType => GroupByEntryTypeAndSelect(entries, statisticsRequest, groupByLevel),
                        GroupBy.Category => GroupByCategoryAndSelect(entries, statisticsRequest, groupByLevel),
                        GroupBy.Tag => GroupByTagAndSelect(entries, statisticsRequest, groupByLevel, dontNegateCostValues),
                        _ => throw new ArgumentException("Unknown value of groupBy")
                    };
                }
                else
                {
                    return new List<StatisticsRowDto>();
                }
            }
            else if (groupByLevel == 0)
            {
                if (entries.Count > 0)
                {
                    return new List<StatisticsRowDto>
                    {
                        new StatisticsRowDto(null, null, null, null, null,
                            SelectValues(entries, statisticsRequest.SelectValues, dontNegateCostValues), null)
                    };
                }
                else
                {
                    return new List<StatisticsRowDto>();
                }
            }
            else
            {
                return new List<StatisticsRowDto>();
            }
        }

        private DateTime FindNextPeriodBoundary(DateTime currentDate,
                                                TimePeriodUnit periodUnit,
                                                int periodValue,
                                                bool goBackwards = false)
        {
            return periodUnit switch
            {
                TimePeriodUnit.Day => currentDate.AddDays(!goBackwards ? periodValue : -periodValue),
                TimePeriodUnit.Month => currentDate.AddMonths(!goBackwards ? periodValue : -periodValue),
                TimePeriodUnit.Year => currentDate.AddYears(!goBackwards ? periodValue : -periodValue),
                _ => throw new ArgumentException("Unknown value of periodUnit")
            };
        }

        private List<StatisticsRowDto> GroupByTimePeriodAndSelect(List<Entry> entries,
                                                                  StatisticsRequestDto statisticsRequest,
                                                                  int groupByLevel,
                                                                  bool dontNegateCostValues)
        {
            if (statisticsRequest.GroupByTimePeriodProperties != null)
            {
                var minEntryDate = entries.Select(entry => entry.Date).Min();
                var maxEntryDate = entries.Select(entry => entry.Date).Max();
                var periodStartDate = statisticsRequest.GroupByTimePeriodProperties.StartDate.Date;
                while (periodStartDate.CompareTo(minEntryDate) > 0)
                {
                    periodStartDate = FindNextPeriodBoundary(
                        periodStartDate,
                        statisticsRequest.GroupByTimePeriodProperties.IntervalUnit,
                        statisticsRequest.GroupByTimePeriodProperties.IntervalValue,
                        true
                        );
                }

                var statisticsRows = new List<StatisticsRowDto>();
                var periodEndDate = periodStartDate.AddTicks(-1);
                do
                {
                    periodEndDate = FindNextPeriodBoundary(
                        periodEndDate,
                        statisticsRequest.GroupByTimePeriodProperties.IntervalUnit,
                        statisticsRequest.GroupByTimePeriodProperties.IntervalValue
                        );

                    var entriesInPeriod = entries.Where(
                        entry => entry.Date.CompareTo(periodStartDate) >= 0 &&
                            entry.Date.CompareTo(periodEndDate) <= 0
                        ).ToList();

                    if (entriesInPeriod.Count > 0)
                    {
                        statisticsRows.Add(CreateStatisticsRow(
                            entriesInPeriod,
                            statisticsRequest,
                            groupByLevel,
                            dontNegateCostValues,
                            periodStartDate,
                            periodEndDate,
                            null,
                            null,
                            null
                            ));
                    }

                    periodStartDate = FindNextPeriodBoundary(
                        periodStartDate,
                        statisticsRequest.GroupByTimePeriodProperties.IntervalUnit,
                        statisticsRequest.GroupByTimePeriodProperties.IntervalValue
                        );
                }
                while (periodEndDate.CompareTo(maxEntryDate) < 0);

                return statisticsRows;
            }
            else
            {
                throw new ArgumentNullException("groupByTimePeriodProperties", "Cannot be null if grouping by timePeriod");
            }
        }

        private List<StatisticsRowDto> GroupByEntryTypeAndSelect(List<Entry> entries,
                                                                 StatisticsRequestDto statisticsRequest,
                                                                 int groupByLevel)
        {
            var statisticsRows = new List<StatisticsRowDto>();
            foreach (var group in entries.OrderBy(entry => entry.Category.IsIncome).GroupBy(entry => entry.Category.IsIncome))
            {
                if (group.Count() > 0)
                {
                    statisticsRows.Add(CreateStatisticsRow(
                        group.ToList(),
                        statisticsRequest,
                        groupByLevel,
                        true,
                        null,
                        null,
                        group.Key ? EntryType.Income : EntryType.Cost,
                        null,
                        null
                        ));
                }
            }
            return statisticsRows;
        }

        private List<StatisticsRowDto> GroupByCategoryAndSelect(List<Entry> entries,
                                                                StatisticsRequestDto statisticsRequest,
                                                                int groupByLevel)
        {
            var statisticsRows = new List<StatisticsRowDto>();
            foreach (var group in entries.OrderBy(entry => entry.Category.OrderOnList).GroupBy(entry => entry.Category.Keyword))
            {
                if (group.Count() > 0)
                {
                    statisticsRows.Add(CreateStatisticsRow(
                        group.ToList(),
                        statisticsRequest,
                        groupByLevel,
                        true,
                        null,
                        null,
                        null,
                        group.Key,
                        null
                        ));
                }
            }
            return statisticsRows;
        }

        private List<StatisticsRowDto> GroupByTagAndSelect(List<Entry> entries,
                                                           StatisticsRequestDto statisticsRequest,
                                                           int groupByLevel,
                                                           bool dontNegateCostValues)
        {
            var statisticsRows = new List<StatisticsRowDto>();
            var tags = entries.SelectMany(entry => entry.EntryTags)
                .Select(entryTag => entryTag.Tag.Name)
                .Distinct()
                .OrderBy(tagName => tagName.ToLower())
                .ToList();
            tags.Add("");
            foreach (var tag in tags)
            {
                var entriesForTag = tag != "" ?
                    entries.Where(entry => entry.EntryTags.Any(entryTag => entryTag.Tag.Name.Equals(tag))).ToList() :
                    entries.Where(entry => entry.EntryTags.Count == 0).ToList();
                if (entriesForTag.Count > 0)
                {
                    statisticsRows.Add(CreateStatisticsRow(
                        entriesForTag,
                        statisticsRequest,
                        groupByLevel,
                        dontNegateCostValues,
                        null,
                        null,
                        null,
                        null,
                        tag
                        ));
                }
            }
            return statisticsRows;
        }

        private StatisticsRowDto CreateStatisticsRow(List<Entry> entries,
                                                     StatisticsRequestDto statisticsRequest,
                                                     int groupByLevel,
                                                     bool dontNegateCostValues,
                                                     DateTime? dateFrom,
                                                     DateTime? dateTo,
                                                     EntryType? entryType,
                                                     string? categoryKeyword,
                                                     string? tagName)
        {
            List<StatisticsRowDto>? subRows = null;
            if (groupByLevel < statisticsRequest.GroupBy.Count - 1)
            {
                subRows = GroupAndSelect(entries, statisticsRequest, groupByLevel + 1, dontNegateCostValues);
            }

            Dictionary<SelectValue, decimal>? values = null;
            if ((statisticsRequest.SelectOnAllLevels.HasValue && statisticsRequest.SelectOnAllLevels.Value) ||
                groupByLevel == statisticsRequest.GroupBy.Count - 1)
            {
                values = SelectValues(entries, statisticsRequest.SelectValues, dontNegateCostValues);
            }

            return new StatisticsRowDto(
                dateFrom,
                dateTo,
                entryType,
                categoryKeyword,
                tagName,
                values,
                subRows
                );
        }

        private Dictionary<SelectValue, decimal> SelectValues(List<Entry> entries, List<SelectValue> selectValues, bool dontNegateCostValues)
        {
            var entriesValues = entries.Select(entry => dontNegateCostValues || entry.Category.IsIncome ? entry.Value : -entry.Value).ToList();
            return selectValues.Select(selectValue =>
            {
                var calculatedValue = 0M;
                if (entries.Count > 0)
                {
                    switch (selectValue)
                    {
                        case SelectValue.Count:
                            calculatedValue = Convert.ToDecimal(entries.Count);
                            break;
                        case SelectValue.Min:
                            calculatedValue = entriesValues.Min();
                            break;
                        case SelectValue.Max:
                            calculatedValue = entriesValues.Max();
                            break;
                        case SelectValue.Sum:
                            calculatedValue = entriesValues.Sum();
                            break;
                        case SelectValue.Average:
                            calculatedValue = Math.Round(entriesValues.Average(), 2);
                            break;
                        case SelectValue.Median:
                            calculatedValue = Math.Round(CalculateMedian(entriesValues), 2);
                            break;
                    }
                }
                return KeyValuePair.Create(selectValue, calculatedValue);
            }).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private decimal CalculateMedian(IEnumerable<decimal> values)
        {
            if (values.Count() > 0)
            {
                var valuesClone = new List<decimal>(values);
                var elementNumberToFind = values.Count() / 2;
                for (int i = 0; i <= elementNumberToFind; i++)
                {
                    var minIndex = i;
                    var minValue = valuesClone[i];
                    for (int j = i + 1; j < valuesClone.Count; j++)
                    {
                        if (valuesClone[j] < minValue)
                        {
                            minIndex = j;
                            minValue = valuesClone[j];
                        }
                    }
                    if (minIndex != i)
                    {
                        var toSwap = valuesClone[i];
                        valuesClone[i] = valuesClone[minIndex];
                        valuesClone[minIndex] = toSwap;
                    }
                }
                return valuesClone.Count % 2 == 0 ?
                    (valuesClone[elementNumberToFind] + valuesClone[elementNumberToFind - 1]) / 2 :
                    valuesClone[elementNumberToFind];
            }
            else
            {
                return 0M;
            }
        }
    }
}
