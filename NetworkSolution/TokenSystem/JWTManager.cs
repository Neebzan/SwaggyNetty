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
        static string key = "Pleadssssssssssssssssssssssssssssssssssssssssh";   //Overvej at gøre brug af asymmetric keys og gem nøglen lokalt på serveren     

        /// <summary>
        /// Generate a JWT token with provided claims that is valid for X amount of days
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="validDays"></param>
        /// <returns></returns>
        public static JwtSecurityToken CreateJWT(ClaimsIdentity claims, int validDays)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateJwtSecurityToken(
                expires: DateTime.UtcNow.AddDays(validDays),
                subject: claims,
                signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256Signature)
                );

            return token;
        }

        /// <summary>
        /// Generate claims for a JWT token with a unique userID and a serializeable object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userID"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ClaimsIdentity CreateClaims<T>(T model) where T : new()
        {
            ClaimsIdentity claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(model.GetType().Name, JsonConvert.SerializeObject(model)));

            return claims;
        }

        /// <summary>
        /// Gets the deserialized object from a JWT claim
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="token"></param>
        /// <returns></returns>
        public static T GetModelFromToken<T>(JwtSecurityToken token)
        {
            return JsonConvert.DeserializeObject<T>(token.Claims.Where(c => c.Type == typeof(T).Name).Select(c => c.Value).FirstOrDefault().ToString());
        }

        /// <summary>
        /// Check if the token is valid and has the correct signature
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool VerifyToken(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            TokenValidationParameters validationParameters = GetTokenValidationParameters();
            try
            {
                ClaimsPrincipal tokenValid = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                return false;
            }

        }

        /// <summary>
        /// Turn a key(string) into a SymmetricSecurityKey
        /// </summary>
        /// <returns></returns>
        private static SecurityKey GetSymmetricSecurityKey()
        {
            byte[] symmetricKey = Encoding.Default.GetBytes(key);
            return new SymmetricSecurityKey(symmetricKey);
        }

        /// <summary>
        /// Get the Token Validation Parameters for the validation process
        /// </summary>
        /// <returns></returns>
        private static TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = GetSymmetricSecurityKey()
            };
        }
    }
}
