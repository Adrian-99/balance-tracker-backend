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
    }
}
