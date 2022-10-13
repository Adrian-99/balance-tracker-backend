using Application.Exceptions;
using Application.Interfaces;
using Application.Settings;
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
        private readonly ITagRepository tagRepository;
        private readonly TagSettings tagSettings;

        public TagService(ITagRepository tagRepository, IConfiguration configuration)
        {
            this.tagRepository = tagRepository;
            tagSettings = TagSettings.Get(configuration);
        }

        public async Task<List<Tag>> GetAllAsync(Guid userId)
        {
            var tags = await tagRepository.GetAllAsync(userId);
            tags.Sort((tag1, tag2) => tag1.Name.ToLower().CompareTo(tag2.Name.ToLower()));
            return tags;
        }

        public async Task<Tag> CreateAsync(Tag tag)
        {
            await ValidateAsync(tag);
            return await tagRepository.AddAsync(tag);
        }

        private async Task ValidateAsync(Tag tag)
        {
            if (tag.Name.Length > tagSettings.Name.MaxLength)
            {
                throw new DataValidationException(
                    $"Tag name must not be longer than {tagSettings.Name.MaxLength} characters"
                    // TODO: Add translation key
                    );
            }
            else if ((await tagRepository.GetByNameIgnoreCaseAsync(tag.UserId, tag.Name)) != null)
            {
                throw new DataValidationException(
                    $"Tag with name {tag.Name} already exists"
                    // TODO: Translation key
                    );
            }
        }
    }
}
