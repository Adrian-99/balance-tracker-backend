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
    public class CategoryRepository : GenericRepository<Category, Guid>, ICategoryRepository
    {
        private readonly DatabaseContext databaseContext;

        public CategoryRepository(DatabaseContext databaseContext)
            : base(databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public override Task<List<Category>> GetAllAsync()
        {
            return databaseContext.Categories
                .OrderBy(c => c.OrderOnList)
                .ToListAsync();
        }

        public Task<Category?> GetByKeywordAsync(string keyword)
        {
            return (from category in databaseContext.Categories
                    where category.Keyword.Equals(keyword)
                    select category).FirstOrDefaultAsync();
        }
    }
}
