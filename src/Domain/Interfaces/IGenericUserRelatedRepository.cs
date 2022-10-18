using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IGenericUserRelatedRepository<Entity, PrimaryKey>
        : IGenericRepository<Entity, PrimaryKey>
        where Entity : AbstractUserRelatedEntity<PrimaryKey>
    {
        Task<Entity?> GetByIdAsync(PrimaryKey entityId, Guid userId);
        Task<bool> CheckIfExistsByIdAsync(PrimaryKey entityId, Guid userId);
    }
}
