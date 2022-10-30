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
    internal class TagService : ITagService
    {
        private static readonly char TAG_NAMES_SEPARATOR = ',';

        private readonly ITagRepository tagRepository;
        private readonly IEntryTagRepository entryTagRepository;
        private readonly TagSettings tagSettings;

        public TagService(ITagRepository tagRepository, IEntryTagRepository entryTagRepository, IConfiguration configuration)
        {
            this.tagRepository = tagRepository;
            this.entryTagRepository = entryTagRepository;
            tagSettings = TagSettings.Get(configuration);
        }

        public async Task<List<Tag>> GetAllAsync(Guid userId)
        {
            var tags = await tagRepository.GetAllAsync(userId);
            tags.Sort((tag1, tag2) => tag1.Name.ToLower().CompareTo(tag2.Name.ToLower()));
            return tags;
        }

        public async Task<Page<Tag>> GetAllPagedAsync(Guid userId, Pageable pageable, TagFilter tagFilter)
        {
            var tags = await tagRepository.GetAllAsync(userId, true);
            var filteredTags = tagFilter.Apply(tags);
            return Page<Tag>.New(pageable, filteredTags);
        }

        public async Task<Tag> CreateAsync(Tag tag)
        {
            await ValidateAsync(tag);
            return await tagRepository.AddAsync(tag);
        }

        public async Task<Tag> UpdateAsync(Guid id, Tag tag)
        {
            var currentTag = await tagRepository.GetByIdAsync(id, tag.UserId);
            if (currentTag != null)
            {
                tag.Id = id;
                await ValidateAsync(tag, !currentTag.Name.ToLower().Equals(tag.Name.ToLower()));
                return await tagRepository.UpdateAsync(currentTag, tag);
            }
            else
            {
                throw new EntityNotFoundException("Tag", id.ToString());
            }

        }

        public async Task DeleteAsync(Guid tagId, Guid userId, string? replacementTags)
        {
            var tag = await tagRepository.GetByIdAsync(tagId, userId);
            if (tag != null)
            {
                var entryTags = await entryTagRepository.GetAllByTagIdAsync(tagId);
                if (entryTags.Count > 0)
                {
                    if (!string.IsNullOrEmpty(replacementTags))
                    {
                        var newTagNames = replacementTags.Split(TAG_NAMES_SEPARATOR);
                        if (newTagNames != null && newTagNames.Count() > 0)
                        {
                            var newTagIds = new List<Guid>();
                            foreach (var tagName in newTagNames)
                            {
                                var newTag = await tagRepository.GetByNameIgnoreCaseAsync(userId, tagName);
                                if (newTag != null)
                                {
                                    newTagIds.Add(newTag.Id);
                                }
                                else
                                {
                                    throw new DataValidationException("Tag with name \"" + tagName + "\" not found");
                                }
                            }

                            foreach (var entryTag in entryTags)
                            {
                                foreach (var newTagId in newTagIds)
                                {
                                    if (!await entryTagRepository.CheckIfExistsAsync(entryTag.EntryId, newTagId))
                                    {
                                        var newEntryTag = new EntryTag();
                                        newEntryTag.EntryId = entryTag.EntryId;
                                        newEntryTag.TagId = newTagId;
                                        await entryTagRepository.AddAsync(newEntryTag);
                                    }
                                }
                            }
                        }
                    }
                    await entryTagRepository.DeleteAsync(entryTags.ToArray());
                }
                await tagRepository.DeleteAsync(tag);
            }
            else
            {
                throw new EntityNotFoundException("Tag", tagId.ToString());
            }
        }

        private async Task ValidateAsync(Tag tag, bool checkIfNameTaken = true)
        {
            if (tag.Name.Length > tagSettings.Name.MaxLength)
            {
                throw new DataValidationException(
                    $"Tag name must not be longer than {tagSettings.Name.MaxLength} characters"
                    // TODO: Add translation key
                    );
            }
            else if (checkIfNameTaken && (await tagRepository.GetByNameIgnoreCaseAsync(tag.UserId, tag.Name)) != null)
            {
                throw new DataValidationException(
                    $"Tag with name {tag.Name} already exists",
                    "error.tag.nameTaken"
                    );
            }
        }
    }
}
