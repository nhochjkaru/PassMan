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
    public class SyncCreateCmdHandler : IRequestHandler<SyncCreateCmd, dtoSyncResponse>
    {
        private readonly IUnitOfWork unitOfWork;

        public SyncCreateCmdHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<dtoSyncResponse> Handle(SyncCreateCmd request, CancellationToken cancellationToken)
        {
            dtoSyncResponse res = new dtoSyncResponse();
            res.resCode = "999";
            using (var transaction = unitOfWork.GetRepository<UserCred>(true).BeginTransaction())
            {
                var UserCredRepo = unitOfWork.GetRepository<UserCred>(true);
                var resA = await UserCredRepo.GetAsync(n => n.userName == request.userName);
                if (resA.Count() > 0)
                {
                    var usercred = resA.FirstOrDefault();
                    res.data = Base64Url.Encode(usercred.cred);
                    res.resCode = "000";
                }
                else
                {
                    res.resCode = "001";
                }
            }
            return res;
        }
    }
}
