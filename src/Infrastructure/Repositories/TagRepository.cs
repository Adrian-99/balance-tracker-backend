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
    public class TagRepository : GenericRepository<Tag, Guid>, ITagRepository
    {
        private readonly DatabaseContext databaseContext;

        public TagRepository(DatabaseContext databaseContext) : base(databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public Task<List<Tag>> GetAllAsync(Guid userId)
        {
            return (from tag in databaseContext.Tags
                    where tag.UserId.Equals(userId)
                    select tag).ToListAsync();
        }

        public async Task<Tag?> GetByNameIgnoreCaseAsync(Guid userId, string name)
        {
            var nameToLower = name.ToLower();
            var userTags = await GetAllAsync(userId);
            return userTags.FirstOrDefault(t => t.Name.ToLower().Equals(nameToLower));
        }
    }
}
