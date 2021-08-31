using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace weatherapi
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
            //services.AddAuthentication("Bearer")
            //    .AddIdentityServerAuthentication("Bearer", options => {
            //        options.ApiName = "weatherapi";
            //        options.Authority = "https://localhost:5443";
            //        options.RequireHttpsMetadata = false;
            //    });

            //To address the audience issue when 

            services.AddAuthentication("Bearer").AddJwtBearer("Bearer",
             options => {
                 options.Authority = "https://localhost:5443";
                 options.Audience = "weatherapi";
                 options.RequireHttpsMetadata = false;

                 options.TokenValidationParameters = new
             TokenValidationParameters() {
                     ValidateAudience = false
                 };
             });

            // can use older IDS4 support
            //services.AddAuthentication()
            //.AddIdentityServerAuthentication("token", options => {
            //    options.RequireHttpsMetadata = false;
            //    options.Authority = "https://localhost:5443";
            //    options.ApiName = "weatherapi";
            //    options.LegacyAudienceValidation = true;
            //});

            //use with [Authorize(AuthenticationSchemes = "token")]



            services.AddControllers();
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "IDS4_Api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IDS4_Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
