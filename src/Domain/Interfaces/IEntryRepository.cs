using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IEntryRepository : IGenericUserRelatedRepository<Entry, Guid>
    {
        Task<List<Entry>> GetAllByCategoryIdAsync(Guid categoryId);
        Task<List<Entry>> GetAllByUserIdAsync(Guid userId);
    }
}
