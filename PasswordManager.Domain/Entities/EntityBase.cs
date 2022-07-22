using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Domain.Entities
{
    public class EntityBase
    {
        [Key] public long Id { get; set; }
        public DateTime insertDate { get; set; }
    }
}
