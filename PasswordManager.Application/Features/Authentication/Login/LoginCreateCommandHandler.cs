using MediatR;
using PasswordManager.Application.Interface;
using PasswordManager.Application.Interface.Login;
using PasswordManager.Domain.Dto;
using PasswordManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PasswordManager.Domain.Dictionary.LoginCmd;

namespace PasswordManager.Application.Features.Authentication.Login
{
    public class LoginCreateCommandHandler : IRequestHandler<LoginCreateCommand, dtoLoginResponse>
    {
        private IUserService _userService;
        private readonly IUnitOfWork unitOfWork;

        public LoginCreateCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<dtoLoginResponse> Handle(LoginCreateCommand request, CancellationToken cancellationToken)
        {
            dtoLoginResponse res = new dtoLoginResponse();

            LoginActivities la = new LoginActivities();
            //la.insertDate = DateTime.Now;
            la.userName = request.req.userName;
            using (var transaction = unitOfWork.GetRepository<User>(true).BeginTransaction())
            {
                var userRepo = unitOfWork.GetRepository<User>(true);
                var resA = await userRepo.GetAsync(n => n.Username == request.req.userName);

                switch (request.req.command)
                {
                    case LCmd.Login:
                        var loginRepo = unitOfWork.GetRepository<LoginActivities>(true);
                        if (resA.Count() > 0)
                        {
                            if (resA.FirstOrDefault().Password == request.req.password)
                            {
                                la.status = "0";
                                res = _userService.Authenticate(resA.FirstOrDefault());
                            }
                            else
                            {
                                la.status = "1";
                            }
                        }
                        else
                        {
                            la.status = "2";
                        }
                        var resB = loginRepo.AddAsync(la);
                        unitOfWork.SaveChanges();
                        transaction.Commit();

                        break;
                    case LCmd.Register:
                        if (resA.Count() > 0)
                        {
                            //user đã tồn tại
                        }
                        else
                        {
                            User u = new User();
                            u.Username= request.req.userName;
                            u.LastName = request.req.LastName;
                            u.FirstName = request.req.FirstName;
                            u.Password = request.req.password;
                            await userRepo.AddAsync(u);
                            unitOfWork.SaveChanges();
                            transaction.Commit();
                        }
                        break;
                    default:
                        break;
                }
            }
            await Task.Delay(500);//
            return res;
        }
    }
}
