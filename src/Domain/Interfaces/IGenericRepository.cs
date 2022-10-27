using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IGenericRepository<Entity, PrimaryKey> where Entity : AbstractEntity<PrimaryKey>
    {
        Task<List<Entity>> GetAllAsync();
        Task<Entity?> GetByIdAsync(PrimaryKey id);
        Task<Entity> AddAsync(Entity entity);
        Task<Entity> UpdateAsync(Entity entity);
        Task<Entity> UpdateAsync(Entity oldEntity, Entity newEntity);
        Task DeleteAsync(Entity entity);
    }
}
