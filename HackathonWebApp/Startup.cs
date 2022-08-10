using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackathonWebApp.Models;
using MongoDB.Driver;

namespace HackathonWebApp
{
    public class Startup
    {
        // Fields
        public IConfiguration Configuration { get; }

        // Constructor
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            CheckConfiguration();
        }
        private void CheckConfiguration()
        {

            // Check that all config values are set
            List<ArgumentNullException> exceptions = new List<ArgumentNullException>();
            if (Configuration["MONGODB_URL"] == null)
                exceptions.Add(new ArgumentNullException("Missing Setting: MONGODB_URL"));
            if (Configuration["MONGODB_IDENTITY_DB_NAME"] == null)
                exceptions.Add(new ArgumentNullException("Missing Setting: MONGODB_IDENTITY_DB_NAME"));
            if (Configuration["MONGODB_HACKATHON_DB_NAME"] == null)
                exceptions.Add(new ArgumentNullException("Missing Setting: MONGODB_HACKATHON_DB_NAME"));
            if (Configuration["EMAIL_USERNAME"] == null)
                exceptions.Add(new ArgumentNullException("Missing Setting: EMAIL_USERNAME"));
            if (Configuration["EMAIL_PASSWORD"] == null)
                exceptions.Add(new ArgumentNullException("Missing Setting: EMAIL_PASSWORD"));

            // Throw exception if any settings are missing
            if (exceptions.Count > 0)
                throw new AggregateException("Missing Configuration", exceptions);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Identity Store
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(Configuration["MONGODB_URL"], Configuration["MONGODB_IDENTITY_DB_NAME"])
                .AddDefaultTokenProviders();

            // Set timeout for tkens
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromHours(24));

            // Add database for collections
            var mongo_client = new MongoClient(Configuration["MONGODB_URL"]);
            services.AddSingleton<IMongoDatabase>(s => {
                return mongo_client.GetDatabase(Configuration["MONGODB_HACKATHON_DB_NAME"]);
            });

            // Add SMTP client for emails
            services.AddSingleton<SmtpClient>(s => {
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(Configuration["EMAIL_USERNAME"], Configuration["EMAIL_PASSWORD"]); // Enter seders User name and password  
                smtp.EnableSsl = true;
                return smtp;
            });

            // Enable serving web pages
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddMvc().AddControllersAsServices();
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
