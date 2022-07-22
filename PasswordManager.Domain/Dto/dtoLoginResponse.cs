using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Domain.Dto
{
    public class dtoLoginResponse
    {
        public string token { get; set; }
        public string userName { get; set; }
        public string name { get; set; }   
        public string cred { get; set; }
        public string resCode { get; set; } 
        public string resDesc { get; set; }

    }
}
