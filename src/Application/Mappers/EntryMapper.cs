using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Application.Exceptions;
using Application.Interfaces;
using Application.Utilities;
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
                entry.Id,
                entry.Date,
                entry.Category.IsIncome ? entry.Value : -entry.Value,
                entry.Name,
                EncryptionUtils.DecryptWithAES(entry.DescriptionContent, entry.DescriptionKey, entry.DescriptionIV),
                entry.Category.Keyword,
                entry.EntryTags.Select(et => TagMapper.FromTagToTagDto(et.Tag)).ToList()
                );
        }

        public async Task<Entry> FromEditEntryDtoToEntryAsync(EditEntryDto editEntryDto, Guid userId)
        {
            var category = await categoryRepository.GetByKeywordAsync(editEntryDto.CategoryKeyword);
            if (category != null)
            {
                string? descriptionKey, descriptionIV;
                var descriptionContent = EncryptionUtils.EncryptWithAES(
                    editEntryDto.Description,
                    out descriptionKey,
                    out descriptionIV);
                
                var entry = new Entry();
                entry.Date = editEntryDto.Date;
                entry.Value = editEntryDto.Value;
                entry.Name = editEntryDto.Name;
                entry.DescriptionContent = descriptionContent;
                entry.DescriptionKey = descriptionKey;
                entry.DescriptionIV = descriptionIV;
                entry.UserId = userId;
                entry.CategoryId = category.Id;
                return entry;
            }
            else
            {
                throw new DataValidationException("Category with keyword \"" + editEntryDto.CategoryKeyword + "\" not found");
            }
        }
    }
}
