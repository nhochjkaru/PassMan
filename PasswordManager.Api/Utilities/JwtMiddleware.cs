namespace PasswordManager.Api.Utilities
{
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using PasswordManager.Application.Interface.Login;
    using PasswordManager.Domain.Entities;
    using PasswordManager.Domain.Models;
    using System.IdentityModel.Tokens.Jwt;
    using System.Text;

    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                attachUserToContext(context, userService, token);

            await _next(context);
        }

        private void attachUserToContext(HttpContext context, IUserService userService, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = long.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                var userName = jwtToken.Claims.First(x => x.Type == "UserName").Value;
                var firstName = jwtToken.Claims.First(x => x.Type == "Firstname").Value;
                var lastName = jwtToken.Claims.First(x => x.Type == "LastName").Value;
                // attach user to context on successful jwt validation
                context.Items["User"] = new User {Id= userId, Username= userName, FirstName=firstName,LastName=lastName};
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}
