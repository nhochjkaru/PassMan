using Jose;
using MediatR;
using PasswordManager.Application.Interface;
using PasswordManager.Domain.Dto;
using PasswordManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Application.Features.Sync
{
    public class UploadCreateCmdHandler : IRequestHandler<UploadCreateCmd, dtoUploadResponse>
    {
        private readonly IUnitOfWork unitOfWork;

        public UploadCreateCmdHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<dtoUploadResponse> Handle(UploadCreateCmd request, CancellationToken cancellationToken)
        {
            dtoUploadResponse res = new dtoUploadResponse();
            res.resCode = "999";
            using (var transaction = unitOfWork.GetRepository<UserCred>(true).BeginTransaction())
            {
                var UserCredRepo = unitOfWork.GetRepository<UserCred>(true);
                var resA = await UserCredRepo.GetAsync(n => n.userName == request.userName);
                if (resA.Count() > 0)
                {
                   var usercred= resA.FirstOrDefault();
                    usercred.cred = Base64Url.Decode(request.req.data);
                    await UserCredRepo.UpdateNoSaveAsync(usercred);
                    unitOfWork.SaveChanges();
                    transaction.Commit();
                    res.resCode = "000";
                }
                else
                {
                    var usercred = new UserCred();
                    usercred.cred= Base64Url.Decode(request.req.data);
                    usercred.userName = request.userName;
                    UserCredRepo.AddNoSaveAsync(usercred);
                    unitOfWork.SaveChanges();
                    transaction.Commit();
                    res.resCode = "000";
                }    
            }
            return res;
        }
    }
}
