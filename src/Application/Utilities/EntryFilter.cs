using Application.Exceptions;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities
{
    public class EntryFilter
    {
        private static readonly char LIST_SEPARATOR = ',';
        private static readonly string DATE = "date";
        private static readonly string VALUE = "value";
        private static readonly string NAME = "name";
        private static readonly string DEFAULT_SORT_BY = $"-{DATE}";

        public string SortBy { get; set; }
        public string? SearchValue { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? CategoriesKeywords { get; set; }
        public string? TagsNames { get; set; }

        public EntryFilter()
        {
            SortBy = DEFAULT_SORT_BY;
        }

        public List<Entry> Apply(List<Entry> entries)
        {
            if (string.IsNullOrEmpty(SortBy) ||
                !Utils.EqualsAnyIgnoreCase(SortBy, DATE, VALUE, NAME, $"-{DATE}", $"-{VALUE}", $"-{NAME}"))
            {
                throw new DataValidationException("Invalid sortBy value");
            }
            if (DateFrom != null && DateTo != null && ((DateTime)DateFrom).CompareTo(DateTo) > 0)
            {
                throw new DataValidationException("Incorrect date range - dateTo must not be before dateFrom");
            }

            if (!string.IsNullOrEmpty(SearchValue))
            {
                entries = entries.FindAll(entry => Utils.AnySourceContainsIgnoreCase(
                    SearchValue,
                    entry.Name,
                    EncryptionUtils.DecryptWithAES(entry.DescriptionContent, entry.DescriptionKey, entry.DescriptionIV)
                    ));
            }
            if (DateFrom != null)
            {
                entries = entries.FindAll(entry => entry.Date.CompareTo(DateFrom) >= 0);
            }
            if (DateTo != null)
            {
                entries = entries.FindAll(entry => entry.Date.CompareTo(DateTo) <= 0);
            }
            if (!string.IsNullOrEmpty(CategoriesKeywords))
            {
                entries = entries.FindAll(entry => Utils.EqualsAnyIgnoreCase(entry.Category.Keyword, GetCategoriesAsArray()));
            }
            if (!string.IsNullOrEmpty(TagsNames))
            {
                entries = entries.FindAll(entry => entry.EntryTags.Any(tag => Utils.EqualsAnyIgnoreCase(tag.Tag.Name, GetTagsAsArray())));
            }

            if (Utils.EndsWithIgnoreCase(SortBy, DATE))
            {
                if (SortBy.StartsWith('-'))
                {
                    entries.Sort((entry1, entry2) => entry2.Date.CompareTo(entry1.Date));
                }
                else
                {
                    entries.Sort((entry1, entry2) => entry1.Date.CompareTo(entry2.Date));
                }
            }
            else if (Utils.EndsWithIgnoreCase(SortBy, VALUE))
            {
                if (SortBy.StartsWith('-'))
                {
                    entries.Sort((entry1, entry2) => {
                        var value1 = entry1.Category.IsIncome ? entry1.Value : -entry1.Value;
                        var value2 = entry2.Category.IsIncome ? entry2.Value : -entry2.Value;
                        return value2.CompareTo(value1);
                        });
                }
                else
                {
                    entries.Sort((entry1, entry2) => {
                        var value1 = entry1.Category.IsIncome ? entry1.Value : -entry1.Value;
                        var value2 = entry2.Category.IsIncome ? entry2.Value : -entry2.Value;
                        return value1.CompareTo(value2);
                    });
                }
            }
            else if (Utils.EndsWithIgnoreCase(SortBy, NAME))
            {
                if (SortBy.StartsWith('-'))
                {
                    entries.Sort((entry1, entry2) => entry2.Name.ToLower().CompareTo(entry1.Name.ToLower()));
                }
                else
                {
                    entries.Sort((entry1, entry2) => entry1.Name.ToLower().CompareTo(entry2.Name.ToLower()));
                }
            }

            return entries;
        }

        private string[] GetCategoriesAsArray()
        {
            return CategoriesKeywords.Split(LIST_SEPARATOR);
        }

        private string[] GetTagsAsArray()
        {
            return TagsNames.Split(LIST_SEPARATOR);
        }
    }
}
