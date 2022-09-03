using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NaeTime.Abstractions.Handlers;
using NaeTime.Core;
using NaeTime.Core.Extensions;
using NaeTime.Core.Models;
using NaeTime.Server.Hubs;
using NaeTime.Server.Services;
using System.Security.Claims;

namespace NaeTime.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.Developer.json", optional: true);
            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddNaeTimeDbContext(options =>
            {
                options.UseSqlServer(connectionString);
            });
            builder.Services.AddNaeTimeUnitOfWork();
            builder.Services.AddNaeTimeValidators();
            builder.Services.AddNaeTimeProcessors();
            builder.Services.AddNaeTimeDtoMapper();
            builder.Services.AddNaeTimeHandlers();

            builder.Services.AddSingleton<RssiStreamBroadcastHandler>();
            builder.Services.AddHostedService(x => x.GetRequiredService<RssiStreamBroadcastHandler>());
            builder.Services.AddSingleton<IRssiStreamReadingHandler>(x => x.GetRequiredService<RssiStreamBroadcastHandler>());

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => { })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

            builder.Services.AddAuthentication()
                .AddIdentityServerJwt();

            builder.Services.Configure<IdentityOptions>(options =>
                options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier);

            builder.Services.AddSignalR();

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<ClientHub>("/clienthub");

            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}