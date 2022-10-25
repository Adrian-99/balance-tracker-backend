using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers
{
    public static class TagMapper
    {
        public static TagDto FromTagToTagDto(Tag tag)
        {
            return new TagDto(tag.Id, tag.Name, tag.EntryTags?.Count);
        }

        public static Tag FromEditTagDtoToTag(Guid userId, EditTagDto editTagDto)
        {
            var tag = new Tag();
            tag.Name = editTagDto.Name;
            tag.UserId = userId;
            return tag;
        }
    }
}
