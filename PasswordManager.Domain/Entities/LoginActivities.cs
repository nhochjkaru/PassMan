using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Domain.Entities
{
    public class LoginActivities : EntityBase
    {
        public string userName { get; set; }
        public string status { get; set; }
        public string ip { get; set; }
        public string machineName { get; set; }
    }
}
