using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Domain.Entities
{
    public class UserCred : EntityBase
    {
        public string userName { get; set; }
        public byte[] cred { get; set; }
    }
}
