using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IEntryTagRepository
    {
        Task<List<EntryTag>> GetAllByEntryIdAsync(Guid entryId);
        Task<EntryTag> AddAsync(EntryTag entryTag);
        Task DeleteAsync(Guid entryId, Guid tagId);
    }
}
