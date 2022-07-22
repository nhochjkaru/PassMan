using MediatR;
using PasswordManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Application.Features.Authentication.Login
{
    public class LoginGetAllQuery : IRequest<IEnumerable<User>>
    {
    }
}
