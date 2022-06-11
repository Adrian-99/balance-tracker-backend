using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("EntryTags")]
    public class EntryTag
    {
        [Required]
        public Guid TagId { get; set; }

        [Required]
        public Guid EntryId { get; set; }


        public virtual Tag Tag { get; set; }
        public virtual Entry Entry { get; set; }
    }
}
