using MediatR;
using PasswordManager.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Application.Features.Authentication.Login
{
    public class LoginCreateCommand : IRequest<dtoLoginResponse>
    {
        public dtoLoginRequest req { get; set; }

        public LoginCreateCommand(dtoLoginRequest req)
        {
            this.req = req ?? throw new ArgumentNullException(nameof(req));
        }
    }
}
