using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Domain.Entities
{
    public class UserCredHistory : EntityBase
    {
        public string userName { get; set; }
        public byte[] cred { get; set; }
        public DateTime effectiveDate { get; set; }
    }
}
