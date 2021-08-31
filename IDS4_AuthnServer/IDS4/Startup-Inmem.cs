using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using IDS4.Data;
using Microsoft.AspNetCore.Identity;

namespace IDS4
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }
              

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddRazorPages();

            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            //https://localhost:5443/.well-known/openid-configuration to see the results in the discovery document
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;


            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();


            services.AddIdentityServer()
                //FOR IN MEMORY STORAGE
                ////.AddInMemoryClients(new List<Client>())
                //.AddInMemoryClients(Config.Clients)
                ////.AddInMemoryIdentityResources(new List<IdentityResource>())
                //.AddInMemoryIdentityResources(Config.IdentityResources)
                ////.AddInMemoryApiResources(new List<ApiResource>())
                //.AddInMemoryApiResources(Config.ApiResources)
                ////.AddInMemoryApiScopes(new List<ApiScope>())
                //.AddInMemoryApiScopes(Config.ApiScopes)


                //.AddTestUsers(new List<TestUser>())
                //.AddTestUsers(Config.Users) - replaced with asp.net identity
                .AddAspNetIdentity<IdentityUser>()




                //extension method provided by ids4
                //Because we are using EF migrations in this quickstart, the call to MigrationsAssembly is used to inform Entity Framework that the host project will contain the migrations code.
                // This is necessary since the host project is in a different assembly than the one that contains the DbContext classes.
                .AddConfigurationStore(options => {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
                       opt => opt.MigrationsAssembly(migrationsAssembly));                
                
                })
                .AddOperationalStore(options => {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
                       opt => opt.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                })

                .AddDeveloperSigningCredential(); //various items in the token needs to be signed - for demo only 

            services.AddControllersWithViews();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();


            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                //endpoints.MapRazorPages();
                endpoints.MapDefaultControllerRoute(); 
            });
        }
    }
}
