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

        public async Task<EntryTag> AddAsync(EntryTag entryTag)
        {
            var addedEntryTag = await databaseContext
                .EntryTags
                .AddAsync(entryTag);
            await databaseContext.SaveChangesAsync();
            return addedEntryTag.Entity;
        }

        public async Task DeleteAsync(Guid entryId, Guid tagId)
        {
            var entryTag = await (from et in databaseContext.EntryTags
                                  where et.EntryId.Equals(entryId) && et.TagId.Equals(tagId)
                                  select et).FirstOrDefaultAsync();
            if (entryTag != null)
            {
                databaseContext.EntryTags.Remove(entryTag);
                await databaseContext.SaveChangesAsync();
            }
        }
    }
}
