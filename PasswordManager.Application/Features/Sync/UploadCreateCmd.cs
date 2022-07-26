using MediatR;
using PasswordManager.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Application.Features.Sync
{
    public class UploadCreateCmd : IRequest<dtoUploadResponse>
    {
        public dtoUploadRequest req { get; set; }
        public string userName { get; set; }

        public UploadCreateCmd(dtoUploadRequest req, string userName)
        {
            this.req = req ?? throw new ArgumentNullException(nameof(req));
            this.userName = userName ?? throw new ArgumentNullException(nameof(userName));
        }
    }
}
