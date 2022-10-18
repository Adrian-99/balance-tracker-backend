using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Tags")]
    public class Tag : AbstractUserRelatedEntity<Guid>
    {
        [Required]
        public string Name { get; set; }


        public virtual ICollection<EntryTag> EntryTags { get; set; }
    }
}
