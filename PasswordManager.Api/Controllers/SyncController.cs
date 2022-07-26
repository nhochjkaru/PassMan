using MediatR;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Api.Utilities;
using PasswordManager.Application.Features.Sync;
using PasswordManager.Domain.Dto;
using PasswordManager.Domain.Entities;
using System.Net;

namespace PasswordManager.Api.Controllers
{
    [Route("api/v1.1/[controller]")]
    [Authorize]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SyncController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        [Route("Upload")]
        [HttpPost]
        [ProducesResponseType(typeof(dtoUploadRequest), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<dtoUploadResponse>> Upload([FromBody] dtoUploadRequest req)
        {
            Console.WriteLine("Upload");
            var user = (User)HttpContext.Items["User"];
            var uploadcmd = new UploadCreateCmd(req, user.Username);
            var uploadRes = await _mediator.Send(uploadcmd);
            return Ok(uploadRes);
        }

        [Route("Sync")]
        [HttpPost]
        [ProducesResponseType(typeof(dtoUploadRequest), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<dtoSyncResponse>> Sync([FromBody] dtoSyncRequest req)
        {
            Console.WriteLine("Sync");
            var user = (User)HttpContext.Items["User"];
            var synccmd = new SyncCreateCmd(req, user.Username);
            var syncRes = await _mediator.Send(synccmd);
            return Ok(syncRes);
        }
    }
}