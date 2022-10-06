using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Entries")]
    public class Entry : AbstractEntity<Guid>
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal Value { get; set; }

        [Required]
        public string Name { get; set; }

        public string? DescriptionContent { get; set; }

        public string? DescriptionKey { get; set; }

        public string? DescriptionIV { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }


        public virtual User User { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<EntryTag> EntryTags { get; set; }
    }
}
