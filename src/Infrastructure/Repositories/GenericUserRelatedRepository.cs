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
    public abstract class GenericUserRelatedRepository<Entity, PrimaryKey>
        : GenericRepository<Entity, PrimaryKey>, IGenericUserRelatedRepository<Entity, PrimaryKey>
        where Entity : AbstractUserRelatedEntity<PrimaryKey>
    {
        public GenericUserRelatedRepository(DatabaseContext databaseContext) : base(databaseContext)
        { }

        public Task<Entity?> GetByIdAsync(PrimaryKey entityId, Guid userId)
        {
            return (from entity in databaseContext.Set<Entity>()
                    where entity.Id.Equals(entityId) && entity.UserId.Equals(userId)
                    select entity).FirstOrDefaultAsync();
        }

        public async Task<bool> CheckIfExistsByIdAsync(PrimaryKey entityId, Guid userId)
        {
            var existingEntity = await (from entity in databaseContext.Set<Entity>()
                                        where entity.Id.Equals(entityId) && entity.UserId.Equals(userId)
                                        select entity).AsNoTracking().FirstOrDefaultAsync();
            return existingEntity != null;
        }

        public override Task<Entity?> GetByIdAsync(PrimaryKey id)
        {
            throw new InvalidOperationException("Use method overload with userId for this entity");
        }
    }
}
