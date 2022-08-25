using Application.Exceptions;
using Application.Interfaces;
using Application.Utilities;
using Application.Utilities.Pagination;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EntryService : IEntryService
    {
        private readonly IEntryRepository entryRepository;
        private readonly IEntryTagRepository entryTagRepository;
        private readonly ITagRepository tagRepository;

        public EntryService(IEntryRepository entryRepository, IEntryTagRepository entryTagRepository, ITagRepository tagRepository)
        {
            this.entryRepository = entryRepository;
            this.entryTagRepository = entryTagRepository;
            this.tagRepository = tagRepository;
        }

        public async Task<Page<Entry>> GetAllPagedAsync(Guid userId, Pageable pageable, EntryFilter entryFilter)
        {
            var entries = await entryRepository.GetAllByUserIdAsync(userId);
            var filteredEntries = entryFilter.Apply(entries);
            return Page<Entry>.New(pageable, filteredEntries);
        }
        public async Task<Entry> CreateAsync(Entry entry, List<string> tagNames)
        {
            var tagIds = new List<Guid>();
            if (tagNames.Count > 0)
            {
                foreach (var tagName in tagNames)
                {
                    var tag = await tagRepository.GetByNameIgnoreCase(entry.UserId, tagName);
                    if (tag != null)
                    {
                        tagIds.Add(tag.Id);
                    }
                    else
                    {
                        throw new DataValidationException("Tag with name \"" + tagName + "\" not found");
                    }
                }
            }
            var addedEntry = await entryRepository.AddAsync(entry);
            if (tagIds.Count > 0)
            {
                foreach (var tagId in tagIds)
                {
                    var entryTag = new EntryTag();
                    entryTag.EntryId = addedEntry.Id;
                    entryTag.TagId = tagId;
                    await entryTagRepository.AddAsync(entryTag);
                }
            }
            return addedEntry;
        }
    }
}
