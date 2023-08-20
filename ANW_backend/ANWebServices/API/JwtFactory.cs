using Amnhac.Interfaces;
using Amnhac.Models;
using ANWebServices.Interfaces;
using ANWebServices.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ANWebServices.API
{
    public class JwtFactory : IJwtFactory
    {
        private JwtOptions _jwtOptions;
        public static string AuthorizationHeader = "Amnhac";

        public JwtFactory(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);
        }

        public JwtOptions GetOption
        {
            get { return _jwtOptions; }
        }
        public ClaimsIdentity GenerateId(string userName, string id, int permission)
        {
            return new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[]
            {
                new Claim("id", id),
                new Claim("rol", permission.ToString())
            });
        }

        /// <summary>
        /// Generate token by using username and claimsIdentity.
        /// </summary>
        /// <param name="userName">Username (Subcriptor)</param>
        /// <param name="id">ClaimsIdentity</param>
        /// <param name="addMinute">Additional token's lifetime in minute</param>
        /// <param name="addHour">Additional token's lifetime in hour</param>
        /// <returns></returns>
        public async Task<string> GetToken(string userName, ClaimsIdentity id, int addMinute = 0, int addHour = 0)
        {
            var claims = new[]
         {
                 new Claim(JwtRegisteredClaimNames.Sub, userName),
                 new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                 new Claim(JwtRegisteredClaimNames.Iat, 
                    ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                 new Claim(ClaimTypes.Role, id.FindFirst("rol").Value),
                 id.FindFirst("id")
             };
            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration.AddMinutes(addMinute).AddHours(addHour),
                signingCredentials: _jwtOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// Convert datetime to miliseconds in the EpoxDate (From Jan/01/1970)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);

        private static void ThrowIfInvalidOptions(JwtOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtOptions.JtiGenerator));
            }
        }
    }
}
