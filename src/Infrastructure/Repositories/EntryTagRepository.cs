using Domain.Entities;
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
    internal class EntryTagRepository : IEntryTagRepository
    {
        private readonly DatabaseContext databaseContext;

        public EntryTagRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task<bool> CheckIfExistsAsync(Guid entryId, Guid tagId)
        {
            var found = await (from entryTag in databaseContext.EntryTags
                               where entryTag.EntryId.Equals(entryId) && entryTag.TagId.Equals(tagId)
                               select entryTag).AsNoTracking().FirstOrDefaultAsync();
            return found != null;
        }

        public Task<List<EntryTag>> GetAllByEntryIdAsync(Guid entryId)
        {
            return (from entryTag in databaseContext.EntryTags
                    where entryTag.EntryId.Equals(entryId)
                    select entryTag).ToListAsync();
        }

        public Task<List<EntryTag>> GetAllByTagIdAsync(Guid tagId)
        {
            return (from entryTag in databaseContext.EntryTags
                    where entryTag.TagId.Equals(tagId)
                    select entryTag).ToListAsync();
        }

        public async Task<EntryTag> AddAsync(EntryTag entryTag)
        {
            var addedEntryTag = await databaseContext.EntryTags.AddAsync(entryTag);
            await databaseContext.SaveChangesAsync();
            return addedEntryTag.Entity;
        }

        public async Task DeleteAsync(params EntryTag[] entryTags)
        {
            databaseContext.EntryTags.RemoveRange(entryTags);
            await databaseContext.SaveChangesAsync();
        }

        public async Task DeleteAllByEntryIdAsync(Guid entryId)
        {
            var entryTags = await GetAllByEntryIdAsync(entryId);
            if (entryTags.Count > 0)
            {
                databaseContext.EntryTags.RemoveRange(entryTags);
                await databaseContext.SaveChangesAsync();
            }
        }
    }
}
