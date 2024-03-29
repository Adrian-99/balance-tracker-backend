﻿using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class TagRepository : GenericUserRelatedRepository<Tag, Guid>, ITagRepository
    {
        public TagRepository(DatabaseContext databaseContext)
            : base(databaseContext)
        { }

        public Task<List<Tag>> GetAllAsync(Guid userId, bool includeEntryTags = false)
        {
            if (includeEntryTags)
            {
                return (from tag in databaseContext.Tags.Include(t => t.EntryTags)
                        where tag.UserId.Equals(userId)
                        select tag).ToListAsync();
            }
            else
            {
                return (from tag in databaseContext.Tags
                        where tag.UserId.Equals(userId)
                        select tag).ToListAsync();
            }
        }

        public async Task<Tag?> GetByNameIgnoreCaseAsync(Guid userId, string name)
        {
            var nameToLower = name.ToLower();
            var userTags = await GetAllAsync(userId);
            return userTags.FirstOrDefault(t => t.Name.ToLower().Equals(nameToLower));
        }

        public override Task<Tag> UpdateAsync(Tag oldEntity, Tag newEntity)
        {
            oldEntity.Name = newEntity.Name;
            oldEntity.UserId = newEntity.UserId;
            return UpdateAsync(oldEntity);
        }
    }
}
