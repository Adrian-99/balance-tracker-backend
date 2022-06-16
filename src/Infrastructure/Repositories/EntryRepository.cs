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
    public class EntryRepository : GenericRepository<Entry, Guid>, IEntryRepository
    {
        private readonly DatabaseContext databaseContext;

        public EntryRepository(DatabaseContext databaseContext)
            : base(databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public Task<List<Entry>> GetAllByCategoryIdAsync(Guid categoryId)
        {
            return (from entry in databaseContext.Entries
                    where entry.CategoryId.Equals(categoryId)
                    select entry).ToListAsync();
        }
    }
}
