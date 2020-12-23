using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Mvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            UseNewtonsoftJson = Configuration["UseNewtonsoftJson"] == "true";
        }

        public IConfiguration Configuration { get; }

        bool UseNewtonsoftJson { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mvcBuilder = services.AddControllers();

            if (UseNewtonsoftJson)
            {
                mvcBuilder.AddNewtonsoftJson();
            }

#if JWTAUTH
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
            {
                o.TokenValidationParameters.ValidateActor = false;
                o.TokenValidationParameters.ValidateLifetime = true;
                o.TokenValidationParameters.ValidateAudience = true;
                o.TokenValidationParameters.ValidateIssuer = true;
                o.TokenValidationParameters.ValidIssuer = "test";
                o.TokenValidationParameters.ValidAudience = "test";
                o.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String("MFswDQYJKoZIhvcNAQEBBQADSgAwRwJAca32BtkpByiveJTwINuEerWBg2kac7sb"));
            });
#elif CERTAUTH
            services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme).AddCertificate(o =>
            {
                o.AllowedCertificateTypes = CertificateTypes.All;
                o.RevocationFlag = X509RevocationFlag.EntireChain;
                o.RevocationMode = X509RevocationMode.NoCheck;
                o.ValidateCertificateUse = false;
                o.ValidateValidityPeriod = false;
            }).AddCertificateCache();

            //services.AddCertificateForwarding(options =>
            //{
            //    options.CertificateHeader = "X-ARR-ClientCert";
            //    options.HeaderConverter = (headerValue) =>
            //    {
            //        X509Certificate2 clientCertificate = null;
            //        if(!string.IsNullOrWhiteSpace(headerValue))
            //        {
            //            byte[] bytes = Convert.FromBase64String(headerValue);
            //            clientCertificate = new X509Certificate2(bytes);
            //            Console.WriteLine("Converted header: "+clientCertificate.Thumbprint);
            //        }
            //        else
            //        {
            //            Console.WriteLine("Empty header");
            //        }
 
            //        return clientCertificate;
            //    };
            //});
#endif

#if AUTHORIZE
            services.AddAuthorization();
#endif

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (UseNewtonsoftJson)
            {
                logger.LogInformation("MVC is configured to use Newtonsoft.Json.");
            }

#if CERTAUTH
            //app.UseHttpsRedirection();
            //app.UseCertificateForwarding();
#endif

            app.UseRouting();

#if JWTAUTH || CERTAUTH
            logger.LogInformation("MVC is configured to use Authentication.");
            app.UseAuthentication();
#endif

#if AUTHORIZE
            logger.LogInformation("MVC is configured to use Authorization.");
            app.UseAuthorization();
#endif

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
