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
    public class EntryRepository : GenericUserRelatedRepository<Entry, Guid>, IEntryRepository
    {
        public EntryRepository(DatabaseContext databaseContext)
            : base(databaseContext)
        { }

        public Task<List<Entry>> GetAllByCategoryIdAsync(Guid categoryId)
        {
            return (from entry in databaseContext.Entries
                    where entry.CategoryId.Equals(categoryId)
                    select entry).ToListAsync();
        }

        public Task<List<Entry>> GetAllByUserIdAsync(Guid userId)
        {
            return (from entry in databaseContext.Entries
                        .Include(e => e.Category)
                        .Include(e => e.EntryTags)
                        .ThenInclude(et => et.Tag)
                    where entry.UserId.Equals(userId)
                    select entry).ToListAsync();
        }

        public override Task<Entry> UpdateAsync(Entry oldEntity, Entry newEntity)
        {
            oldEntity.Date = newEntity.Date;
            oldEntity.Value = newEntity.Value;
            oldEntity.Name = newEntity.Name;
            oldEntity.DescriptionContent = newEntity.DescriptionContent;
            oldEntity.DescriptionKey = newEntity.DescriptionKey;
            oldEntity.DescriptionIV = newEntity.DescriptionIV;
            oldEntity.CategoryId = newEntity.CategoryId;
            oldEntity.UserId = newEntity.UserId;
            return UpdateAsync(oldEntity);
        }
    }
}
