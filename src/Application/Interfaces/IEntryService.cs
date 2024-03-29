﻿using Application.Utilities;
using Application.Utilities.Pagination;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEntryService
    {
        Task<Page<Entry>> GetAllPagedAsync(Guid userId, Pageable pageable, EntryFilter entryFilter);
        Task<Entry> CreateAsync(Entry entry, List<string> tagNames);
        Task<Entry> UpdateAsync(Guid id, Entry entry, List<string> tagNames);
        Task DeleteAsync(Guid id, Guid userId);
        void ValidateDescription(string? entryDescription);
        Task AssertEntryExistsAsync(Guid entryId, Guid userId);
    }
}
