using PasswordManager.Domain.Dictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Domain.Dto
{
    public class dtoLoginRequest
    {
        public LoginCmd.LCmd command { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
