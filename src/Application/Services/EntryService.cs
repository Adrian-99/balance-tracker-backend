using Application.Interfaces;
using Application.Utilities;
using Application.Utilities.Pagination;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EntryService : IEntryService
    {
        private readonly IEntryRepository entryRepository;

        public EntryService(IEntryRepository entryRepository)
        {
            this.entryRepository = entryRepository;
        }

        public async Task<Page<Entry>> GetAllPagedAsync(Guid userId, Pageable pageable, EntryFilter entryFilter)
        {
            var entries = await entryRepository.GetAllByUserIdAsync(userId);
            var filteredEntries = entryFilter.Apply(entries);
            return Page<Entry>.New(pageable, filteredEntries);
        }
    }
}
