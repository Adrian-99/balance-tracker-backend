using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
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

        public TagService(ITagRepository tagRepository)
        {
            this.tagRepository = tagRepository;
        }

        public async Task<List<Tag>> GetAllAsync(Guid userId)
        {
            var tags = await tagRepository.GetAllAsync(userId);
            tags.Sort((tag1, tag2) => tag1.Name.ToLower().CompareTo(tag2.Name.ToLower()));
            return tags;
        }

        public async Task<Tag> CreateAsync(Tag tag)
        {
            if ((await tagRepository.GetByNameIgnoreCaseAsync(tag.UserId, tag.Name)) != null)
            {
                throw new DataValidationException(
                    $"Tag with name {tag.Name} already exists"
                    // TODO: Translation key
                    );
            }
            return await tagRepository.AddAsync(tag);
        }
    }
}
