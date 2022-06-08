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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Load URL to NOSQL database (MongoDB)
            string MONGODB_URL = null;
            if (MONGODB_URL == null)
                MONGODB_URL = Configuration.GetValue<string>("MONGODB_URL");
            if (MONGODB_URL == null)
                MONGODB_URL = Environment.GetEnvironmentVariable("MONGODB_URL");
            if (MONGODB_URL == null)
                throw new ArgumentNullException("Missing Setting: MONGODB_URL");

            // Add DB link to Identity Store
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(MONGODB_URL, "Identity")
                .AddTokenProvider("Default", typeof(EmailTwoFactorAuthentication<ApplicationUser>));

            // Add client to MongoDB DBs
            services.AddSingleton<IMongoClient, MongoClient>(s => {
                return new MongoClient(MONGODB_URL);
            });

            // Load credentials for email account
            string EMAIL_USERNAME = null;
            if (EMAIL_USERNAME == null)
                EMAIL_USERNAME = Configuration.GetValue<string>("EMAIL_USERNAME");
            if (EMAIL_USERNAME == null)
                EMAIL_USERNAME = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
            if (EMAIL_USERNAME == null)
                throw new ArgumentNullException("Missing Setting: EMAIL_USERNAME");
            string EMAIL_PASSWORD = null;
            if (EMAIL_PASSWORD == null)
                EMAIL_PASSWORD = Configuration.GetValue<string>("EMAIL_PASSWORD");
            if (EMAIL_PASSWORD == null)
                EMAIL_PASSWORD = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
            if (EMAIL_PASSWORD == null)
                throw new ArgumentNullException("Missing Setting: EMAIL_PASSWORD");

            // Add SMTP client for emails
            services.AddSingleton<SmtpClient>(s => {
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(EMAIL_USERNAME, EMAIL_PASSWORD); // Enter seders User name and password  
                smtp.EnableSsl = true;
                return smtp;
            });

            // Enable serving web pages
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
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
