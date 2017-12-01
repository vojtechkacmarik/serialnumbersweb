using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SerialNumbers.Extensions;
using SerialNumbers.Web.Data;
using SerialNumbers.Web.Models;
using SerialNumbers.Web.Services;
using SerialNumbers.Web.Settings;
using Serilog;

namespace SerialNumbers.Web
{
    public class Startup
    {
        private const string SECRET_KEY = "MySecret";
        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Startup>();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            _logger.LogInformation("Configuration started ...");
            _logger.LogInformation($"UserSecrets: {SECRET_KEY}={Configuration[SECRET_KEY]}");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            ConfigureApplicationLifetime(appLifetime);
            _logger.LogInformation("Configuration completed.");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            _logger.LogInformation($"{services.Count} services are configured.");

            var connectionString = Environment.GetEnvironmentVariable("SQLSERVER_CONNECTIONSTRING");
            var internalConnectionString = connectionString ?? Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(internalConnectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            ConfigureApplicationServices(services);
            ConfigureSerialNumbers(services, internalConnectionString);

            services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddMvc();

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.BuildDatabase();
            BuildDatabase<ApplicationDbContext>(serviceProvider);
            return serviceProvider;
        }

        private static void ConfigureSerialNumbers(IServiceCollection services, string connectionString)
        {
            services.AddSerialNumbers(connectionString);
            services.AddSerialNumbersLocalDateTimeProvider();
        }

        private static void ConfigureApplicationServices(IServiceCollection services)
        {
            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
        }

        public static void BuildDatabase<TDbContext>(ServiceProvider serviceProvider) where TDbContext : DbContext
        {
            var serviceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            using (var serviceScope = serviceScopeFactory.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<TDbContext>();
                dbContext.Database.Migrate();
            }
        }

        private void ConfigureApplicationLifetime(IApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                appLifetime.StopApplication();
                // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };
        }

        private void OnStarted()
        {
            _logger.LogInformation("OnStarted");
            // Perform post-startup activities here
        }

        private void OnStopped()
        {
            _logger.LogInformation("OnStopped");
            // Perform post-stopped activities here
        }

        private void OnStopping()
        {
            _logger.LogInformation("OnStopping");
            // Perform on-stopping activities here
            Log.CloseAndFlush();
            Thread.Sleep(500);
        }
    }
}