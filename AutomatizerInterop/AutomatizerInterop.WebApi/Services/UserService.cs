using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using AutomatizerInterop.WebApi.Helper;
using AutomatizerInterop.Data.Interfaces;

namespace AutomatizerInterop.WebApi.Services
{
    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        //private List<User> _users = new List<User>
        //{
        //    new User { Id = 1, FirstName = "Test", LastName = "User", Username = "test", Password = "test" }
        //};

        private readonly AppSettings _appSettings;
        private readonly IUsuariosRepository usuariosRepository;

        public UserService(IOptions<AppSettings> appSettings, IUsuariosRepository usuariosRepository)
        {
            _appSettings = appSettings.Value;
            this.usuariosRepository = usuariosRepository;
        }

        public User Authenticate(string username, string password)
        {
            var user = usuariosRepository.GetUser(username, password); //_users.SingleOrDefault(x => x.Username == username && x.Password == password);

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim( ClaimTypes.NameIdentifier, user.Id.ToString() ),
                    new Claim(ClaimTypes.Name, user.FirstName + ' '  + user.LastName),
                    new Claim(ClaimTypes.GivenName, user.Username),
                
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
            

            return user.WithoutPassword();
        }

    }
}
