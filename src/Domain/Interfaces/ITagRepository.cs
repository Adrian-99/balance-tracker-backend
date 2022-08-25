using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITagRepository : IGenericRepository<Tag, Guid>
    {
        Task<List<Tag>> GetAll(Guid userId);
        Task<Tag?> GetByNameIgnoreCase(Guid userId, string name);
    }
}
