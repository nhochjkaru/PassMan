using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Domain.Dictionary
{
    public class LoginCmd
    {
        public enum LCmd
        {
            Login,
            Register,
            ChangePassword
        }
    }
}
