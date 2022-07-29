using MediatR;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Api.Utilities;
using PasswordManager.Application.Features.Authentication.Login;
using PasswordManager.Domain.Dto;
using PasswordManager.Domain.Entities;
using System.Net;
using static PasswordManager.Domain.Dictionary.LoginCmd;

namespace PasswordManager.Api.Controllers
{
    [Route("api/v1.1/[controller]")]
    //[Authorize]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthenticationController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet("download")]
        public IActionResult GetBlobDownload([FromQuery] string link)
        {
            var net = new System.Net.WebClient();
            var data = net.DownloadData(link);
            var content = new System.IO.MemoryStream(data);
            var contentType = "APPLICATION/octet-stream";
            var fileName = "something.bin";
            return File(content, contentType, fileName);
        }

        [Route("login")]
        [HttpPost]
        [ProducesResponseType(typeof(dtoLoginResponse), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<dtoLoginResponse>> login([FromBody] dtoLoginRequest req)
        {
            req.command = LCmd.Login;
            var login = new LoginCreateCommand(req);
            var loginRes = await _mediator.Send(login);
            if(loginRes.resCode == "000") 
            {
                loginRes.resDesc = "Success";
            };
            return Ok(loginRes);
        }

        [Route("register")]
        [HttpPost]
        [ProducesResponseType(typeof(dtoLoginResponse), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<dtoLoginResponse>> register([FromBody] dtoLoginRequest req)
        {
            req.command = LCmd.Register;
            var login = new LoginCreateCommand(req);
            var loginRes = await _mediator.Send(login);
            if (loginRes.resCode == "000")
            {
                loginRes.resDesc = "Success";
            };
            return Ok(loginRes);
        }

        [Route("changePass")]
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(dtoLoginResponse), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<dtoLoginResponse>> changePass([FromBody] dtoLoginRequest req)
        {
            var user = (User)HttpContext.Items["User"];
            req.command = LCmd.ChangePassword;
            req.userName = user.Username;
            var login = new LoginCreateCommand(req);
            var loginRes = await _mediator.Send(login);
            if (loginRes.resCode == "000")
            {
                loginRes.resDesc = "Success";
            };
            return Ok(loginRes);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var user = (User)HttpContext.Items["User"];
            var login = new LoginGetAllQuery();
            var users = await _mediator.Send(login);
            return Ok(users);
        }
        [Route("ValidToken")]
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> ValidToken()
        {
            var user = (User)HttpContext.Items["User"];
            return Ok(user.Username);
        }
    }
}
