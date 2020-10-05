using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Entities.Identity;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        //Only the secret key on the server is used to encrypt and decrypt this key
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            _config = config;
            //The token needs to be validated with each request to the server.
            //Create key based on token in the appsetting.json file.
            //Server uses this secret key to ensure the signature of the token and so this needs
            //to be stored safely in production
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Token:Key"]));

        }
        public string CreateToken(AppUser user)
        {
            //Build the JWT token and with this token, the server need not go to the database to check
            //its validity and this is reduces the overhead of each API call of doing an I/O to the DB.
            
            //The (1)claims in the token need to enable this and in this case will have 2 claims

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.DisplayName)
            };

            //The (2)signing credentials combines the security key generated from the token and uses an algorithm
            //we choose to hash the key
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            //Create token descriptor that will contain (1)claims, expiry date (7 days) for the token and
            //(2)signing credentials
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["Token:Issuer"]
            };

            //Create a JWT token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            //Using token handler, a token can be created passing the token descriptor
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}