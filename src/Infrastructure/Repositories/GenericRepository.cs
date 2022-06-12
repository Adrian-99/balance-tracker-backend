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
    public abstract class GenericRepository<Entity, PrimaryKey>
        : IGenericRepository<Entity, PrimaryKey>
        where Entity : AbstractEntity<PrimaryKey>
    {
        private readonly DatabaseContext databaseContext;

        public GenericRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task<Entity> AddAsync(Entity entity)
        {
            var addedEntity = await databaseContext
                .Set<Entity>()
                .AddAsync(entity);
            await databaseContext.SaveChangesAsync();
            return addedEntity.Entity;
        }

        public async Task DeleteAsync(PrimaryKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                databaseContext
                    .Set<Entity>()
                    .Remove(entity);
                await databaseContext.SaveChangesAsync();
            }
        }

        public Task<List<Entity>> GetAllAsync()
        {
            return databaseContext
                .Set<Entity>()
                .ToListAsync();
        }

        public async Task<Entity?> GetByIdAsync(PrimaryKey id)
        {
            return await (from entity in databaseContext.Set<Entity>()
                         where entity.Id.Equals(id)
                         select entity).FirstOrDefaultAsync();
        }

        public async Task<Entity> UpdateAsync(Entity entity)
        {
            var updatedEntity = databaseContext
                .Set<Entity>()
                .Update(entity);
            await databaseContext.SaveChangesAsync();
            return updatedEntity.Entity;
        }
    }
}
