using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using HeadHuntery.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Principal;

namespace HeadHuntery.Api
{
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly JwtTokenOptions _options;

        public TokenProviderMiddleware(RequestDelegate requestDelegate, IOptions<JwtTokenOptions> options)
        {
            this._requestDelegate = requestDelegate;
            this._options = options.Value;
        }

        public Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.Equals(this._options.Path, StringComparison.Ordinal))
                return this._requestDelegate(context);

            if (!context.Request.Method.Equals("POST") || !context.Request.HasFormContentType)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return context.Response.WriteAsync(CommonResponse.FromError("Bad request."));
            }

            return GenerateToken(context);
        }

        private async Task GenerateToken(HttpContext context)
        {
            var username = context.Request.Form["username"];
            var password = context.Request.Form["pwhash"];

            var identity = await GetIdentity(username, password);
            if (identity == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(CommonResponse.FromError("Invalid username or password."));
                return;
            }

            var now = DateTimeOffset.UtcNow;

            var claims = new []
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var jwt = new JwtSecurityToken(
                issuer: this._options.Issuer,
                audience: this._options.Audience,
                expires: now.Add(this._options.Expiration).DateTime,
                signingCredentials: this._options.SigningCredentials,
                claims: claims,
                notBefore: now.DateTime
            );

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var tokenResponse = new TokenResponse(encodedJwt, (int)this._options.Expiration.TotalSeconds);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(tokenResponse, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        private Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            if (username == "test" && password == "test2")
                return Task.FromResult(new ClaimsIdentity(new GenericIdentity(username, "Token"), new Claim[]{}));

            return Task.FromResult<ClaimsIdentity>(null);
        }
    }
}