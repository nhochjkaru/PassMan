using Microsoft.IdentityModel.Tokens;
using PasswordManager.Domain.Dto;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace PasswordManager.Application.Interface.Login
{
    public interface IUserService
    {
        dtoLoginResponse Authenticate(User model);
        //IEnumerable<User> GetAll();
        //User GetById(int id);
    }
    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        //private List<User> _users = new List<User>
        //{
        //    new User { Id = 1, FirstName = "Test", LastName = "User", Username = "test", Password = "test" }
        //};

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public dtoLoginResponse Authenticate(User model)
        {
            //var user = _users.SingleOrDefault(x => x.Username == model.userName && x.Password == model.password);
            var user = model;
            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt token
            var token = generateJwtToken(user);

            return new dtoLoginResponse { resCode = "000", userName = model.Username, token = token };
        }

        //public IEnumerable<User> GetAll()
        //{
        //    return _users;
        //}

        //public User GetById(string id)
        //{
        //    return _users.FirstOrDefault(x => x.Id == id);
        //}

        // helper methods

        private string generateJwtToken(User user)
        {
            // generate token that is valid for 7 days
            user.Password = "masked";
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()), 
                    new Claim("UserName", user.Username.ToString()),
                new Claim("Firstname", user.FirstName.ToString()),
                new Claim("LastName", user.LastName.ToString())}),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
