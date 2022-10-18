using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public abstract class AbstractUserRelatedEntity<PrimaryKey> : AbstractEntity<PrimaryKey>
    {
        [Required]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }
    }
}
