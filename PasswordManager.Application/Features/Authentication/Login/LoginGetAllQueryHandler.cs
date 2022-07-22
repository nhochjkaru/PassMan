using MediatR;
using PasswordManager.Application.Interface.Login;
using PasswordManager.Domain.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Application.Features.Authentication.Login
{
    public class LoginGetAllQueryHandler : IRequestHandler<LoginGetAllQuery, IEnumerable<User>>
    {
        private IUserService _userService;

        public LoginGetAllQueryHandler(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<IEnumerable<User>> Handle(LoginGetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<User> users = null;// _userService.GetAll();
            return users;
        }
    }
}
