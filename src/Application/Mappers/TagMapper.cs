using Application.Dtos;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers
{
    public class TagMapper
    {
        public static TagDto FromTagToTagDto(Tag tag)
        {
            return new TagDto(tag.Name);
        }

        public static List<TagDto> FromTagToTagDto(List<Tag> tags)
        {
            return tags.Select(t => FromTagToTagDto(t)).ToList();
        }

        public static Tag FromTagDtoToTag(Guid userId, TagDto tagDto)
        {
            var tag = new Tag();
            tag.Name = tagDto.Name;
            tag.UserId = userId;
            return tag;
        }
    }
}
