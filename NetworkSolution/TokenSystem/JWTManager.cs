using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TokenSystem
{
    public static class JWTManager
    {
        static string key = "Pleadssssssssssssssssssssssssssssssssssssssssh";        

        public static JwtSecurityToken CreateJWT(ClaimsIdentity claims, int validDays)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateJwtSecurityToken(
                expires: DateTime.UtcNow.AddDays(validDays),
                subject: claims,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.Default.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
                );

            return token;
        }

        public static ClaimsIdentity CreateClaims<T>(string userID, T model) where T : new()
        {
            ClaimsIdentity claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.Name, userID));
            claims.AddClaim(new Claim(model.GetType().Name, JsonConvert.SerializeObject(model)));

            return claims;
        }

        public static T GetModelFromToken<T>(JwtSecurityToken token)
        {
            return JsonConvert.DeserializeObject<T>(token.Claims.Where(c => c.Type == typeof(T).Name).Select(c => c.Value).FirstOrDefault().ToString());
        }
    }
}
