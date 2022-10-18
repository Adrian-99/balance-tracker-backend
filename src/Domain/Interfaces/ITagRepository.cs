using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITagRepository : IGenericUserRelatedRepository<Tag, Guid>
    {
        Task<List<Tag>> GetAllAsync(Guid userId);
        Task<Tag?> GetByNameIgnoreCaseAsync(Guid userId, string name);
    }
}
