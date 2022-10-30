using Application.Utilities;
using Application.Utilities.Pagination;
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
        Task<Page<Tag>> GetAllPagedAsync(Guid userId, Pageable pageable, TagFilter tagFilter);
        Task<Tag> CreateAsync(Tag tag);
        Task<Tag> UpdateAsync(Guid id, Tag tag);
        Task DeleteAsync(Guid tagId, Guid userId, string? replacementTags);
    }
}
