using Application.Exceptions;
using Application.Interfaces;
using Application.Settings;
using Application.Utilities;
using Application.Utilities.Pagination;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
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
        private readonly EntrySettings entrySettings;

        public EntryService(IEntryRepository entryRepository,
                            IEntryTagRepository entryTagRepository,
                            ITagRepository tagRepository,
                            IConfiguration configuration)
        {
            this.entryRepository = entryRepository;
            this.entryTagRepository = entryTagRepository;
            this.tagRepository = tagRepository;
            entrySettings = EntrySettings.Get(configuration);
        }

        public async Task<Page<Entry>> GetAllPagedAsync(Guid userId, Pageable pageable, EntryFilter entryFilter)
        {
            var entries = await entryRepository.GetAllByUserIdAsync(userId);
            var filteredEntries = entryFilter.Apply(entries);
            return Page<Entry>.New(pageable, filteredEntries);
        }

        public async Task<Entry> CreateAsync(Entry entry, List<string> tagNames)
        {
            var tagIds = await ValidateAndGetTagIdsAsync(entry, tagNames);
            var addedEntry = await entryRepository.AddAsync(entry);
            if (tagIds.Count > 0)
            {
                foreach (var tagId in tagIds)
                {
                    await AddNewEntryTagAsync(addedEntry.Id, tagId);
                }
            }
            return addedEntry;
        }

        public async Task<Entry> UpdateAsync(Guid id, Entry entry, List<string> tagNames)
        {
            var tagIds = await ValidateAndGetTagIdsAsync(entry, tagNames);
            entry.Id = id;
            var updatedEntry = await entryRepository.UpdateAsync(entry);
            var entryTags = await entryTagRepository.GetAllByEntryIdAsync(entry.Id);
            if (tagIds.Count > 0)
            {
                var entryTagsToDelete = new List<EntryTag>();
                var entryTagIdsToAdd = new List<Guid>(tagIds);
                foreach (var entryTag in entryTags)
                {
                    if (tagIds.Contains(entryTag.TagId))
                    {
                        entryTagIdsToAdd.Remove(entryTag.TagId);
                    }
                    else
                    {
                        entryTagsToDelete.Add(entryTag);
                    }
                }
                if (entryTagsToDelete.Count > 0)
                {
                    foreach (var entryTag in entryTagsToDelete)
                    {
                        await entryTagRepository.DeleteAsync(entryTag);
                    }
                }
                if (entryTagIdsToAdd.Count > 0)
                {
                    foreach (var tagId in entryTagIdsToAdd)
                    {
                        await AddNewEntryTagAsync(updatedEntry.Id, tagId);
                    }
                }
            }
            else
            {
                if (entryTags.Count > 0)
                {
                    foreach (var entryTag in entryTags)
                    {
                        await entryTagRepository.DeleteAsync(entryTag);
                    }
                }
            }
            return updatedEntry;
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var entry = await entryRepository.GetByIdAsync(id, userId);
            if (entry != null)
            {
                await entryTagRepository.DeleteAllByEntryIdAsync(id);
                await entryRepository.DeleteAsync(entry);
            }
            else
            {
                throw new EntityNotFoundException("Entry", id.ToString());
            }
        }

        public void ValidateDescription(string? entryDescription)
        {
            if (entryDescription?.Length > entrySettings.Description.MaxLength)
            {
                throw new DataValidationException(
                    $"Entry description must not be longer than {entrySettings.Description.MaxLength} characters"
                    // TODO: Add translation key
                    );
            }
        }

        public async Task AssertEntryExistsAsync(Guid entryId, Guid userId)
        {
            if (!await entryRepository.CheckIfExistsByIdAsync(entryId, userId))
            {
                throw new EntityNotFoundException("Entry", entryId.ToString());
            }
        }

        private async Task<List<Guid>> ValidateAndGetTagIdsAsync(Entry entry, List<string> tagNames)
        {
            if (entry.Name.Length > entrySettings.Name.MaxLength)
            {
                throw new DataValidationException(
                    $"Entry name must not be longer than {entrySettings.Name.MaxLength} characters"
                    // TODO: Add translation key
                    );
            }

            var tagIds = new List<Guid>();
            if (tagNames.Count > 0)
            {
                foreach (var tagName in tagNames)
                {
                    var tag = await tagRepository.GetByNameIgnoreCaseAsync(entry.UserId, tagName);
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
            return tagIds;
        }

        private async Task AddNewEntryTagAsync(Guid entryId, Guid tagId)
        {
            var entryTag = new EntryTag();
            entryTag.EntryId = entryId;
            entryTag.TagId = tagId;
            await entryTagRepository.AddAsync(entryTag);
        }
    }
}
