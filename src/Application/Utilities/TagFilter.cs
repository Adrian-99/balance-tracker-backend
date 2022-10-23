using Application.Exceptions;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities
{
    public class TagFilter
    {
        private static readonly string NAME = "name";
        private static readonly string ENTRIES_COUNT = "entriesCount";
        private static readonly string DEFAULT_SORT_BY = NAME;

        public string SortBy { get; set; }
        public string? SearchValue { get; set; }

        public TagFilter()
        {
            SortBy = DEFAULT_SORT_BY;
        }

        public List<Tag> Apply(List<Tag> tags)
        {
            if (string.IsNullOrEmpty(SortBy) ||
                !Utils.EqualsAnyIgnoreCase(SortBy, NAME, ENTRIES_COUNT, $"-{NAME}", $"-{ENTRIES_COUNT}"))
            {
                throw new DataValidationException("Invalid sortBy value");
            }

            if (!string.IsNullOrEmpty(SearchValue))
            {
                tags = tags.FindAll(t => Utils.AnySourceContainsIgnoreCase(SearchValue, t.Name));
            }

            if (Utils.EndsWithIgnoreCase(SortBy, NAME))
            {
                if (SortBy.StartsWith('-'))
                {
                    tags.Sort((tag1, tag2) => tag2.Name.ToLower().CompareTo(tag1.Name.ToLower()));
                }
                else
                {
                    tags.Sort((tag1, tag2) => tag1.Name.ToLower().CompareTo(tag2.Name.ToLower()));
                }
            }
            else if (Utils.EndsWithIgnoreCase(SortBy, ENTRIES_COUNT))
            {
                if (SortBy.StartsWith('-'))
                {
                    tags.Sort((tag1, tag2) => tag2.EntryTags.Count().CompareTo(tag1.EntryTags.Count()));
                }
                else
                {
                    tags.Sort((tag1, tag2) => tag1.EntryTags.Count().CompareTo(tag2.EntryTags.Count()));
                }
            }

            return tags;
        }
    }
}
