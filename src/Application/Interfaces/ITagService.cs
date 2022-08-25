using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITagService
    {
        Task<List<Tag>> GetAllAsync(Guid userId);
        Task<Tag> CreateAsync(Tag tag);
    }
}
