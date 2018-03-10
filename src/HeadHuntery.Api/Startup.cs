using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HeadHuntery.Api.Models;

namespace HeadHuntery.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var secretKey = Configuration["TokenAuthentication:SecretKey"];
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

            services.AddMvc();
            services.AddAuthentication().AddJwtBearer(x => 
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = "UserTokenIssuer",
                    ValidateAudience = true,
                    ValidAudience = "UserTokenAudience",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var secretKey = Configuration["TokenAuthentication:SecretKey"];
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

            var options = new JwtTokenOptions
            {
                Audience = "UserTokenAudience",
                Issuer = "UserTokenIssuer",
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
            };

            app.UseMiddleware<TokenProviderMiddleware>(Options.Create(options));

            app.UseMvc();
        }
    }
}
