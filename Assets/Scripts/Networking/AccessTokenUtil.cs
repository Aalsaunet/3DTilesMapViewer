//using FourPro.Configuration.Azure;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FourPro.Web.SignalR
{
    internal class AccessTokenUtil
    {
        private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();

        private readonly string _accessKey;
        private readonly SymmetricSecurityKey _securityKey;
        private readonly SigningCredentials _credentials;

        public AccessTokenUtil(string accessKey)
        {
            _accessKey = accessKey;
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_accessKey));
            _credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
        }

        public AccessToken GenerateAccessToken(string audience, string userId, TimeSpan? lifetime = null)
        {
            IEnumerable<Claim> claims = null;
            if (userId != null)
            {
                claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                };
            }

            return GenerateAccessTokenInternal(audience, claims, lifetime ?? TimeSpan.FromHours(1));
        }

        public AccessToken GenerateAccessTokenInternal(string audience, IEnumerable<Claim> claims, TimeSpan lifetime)
        {
            var expire = DateTime.UtcNow.Add(lifetime);
            var token = JwtTokenHandler.CreateJwtSecurityToken(
                issuer: null,
                audience: audience,
                subject: claims == null ? null : new ClaimsIdentity(claims),
                expires: expire,
                signingCredentials: _credentials);
            return new AccessToken(expire, JwtTokenHandler.WriteToken(token));
        }

        public class AccessToken
        {
            public AccessToken(DateTime expires, string token)
            {
                Expires = expires;
                Token = token;
            }

            public DateTime Expires { get; private set; }

            public string Token { get; private set; }

            public bool IsExpired()
            {
                var dt = DateTime.UtcNow;
                return dt > Expires.AddMinutes(-1);
            }
        }
    }
}
