using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers
{
    public class EntryMapper : IEntryMapper
    {
        private readonly ICategoryRepository categoryRepository;

        public EntryMapper(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public EntryDto FromEntryToEntryDto(Entry entry)
        {
            return new EntryDto(
                entry.Date,
                entry.Value,
                entry.Name,
                entry.Description,
                entry.Category.Keyword,
                entry.EntryTags.Select(et => TagMapper.FromTagToTagDto(et.Tag)).ToList()
                );
        }

        public async Task<Entry> FromEntryDtoToEntryAsync(EntryDto entryDto, Guid userId)
        {
            var category = await categoryRepository.GetByKeywordAsync(entryDto.CategoryKeyword);
            if (category != null)
            {
                var entry = new Entry();
                entry.Date = entryDto.Date;
                entry.Value = entryDto.Value;
                entry.Name = entryDto.Name;
                entry.Description = entryDto.Description;
                entry.UserId = userId;
                entry.CategoryId = category.Id;
                return entry;
            }
            else
            {
                throw new DataValidationException("Category with keyword \"" + entryDto.CategoryKeyword + "\" not found");
            }
        }
    }
}
