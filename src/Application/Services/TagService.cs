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

        public async Task<List<Tag>> GetAll(Guid userId)
        {
            var tags = await tagRepository.GetAll(userId);
            tags.Sort((tag1, tag2) => tag1.Name.ToLower().CompareTo(tag2.Name.ToLower()));
            return tags;
        }
    }
}
