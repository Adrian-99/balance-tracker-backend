﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Categories")]
    public class Category : AbstractEntity<Guid>
    {
        [Required]
        public string NameTranslationKey { get; set; }

        [Required]
        public bool IsIncome { get; set; }
    }
}
