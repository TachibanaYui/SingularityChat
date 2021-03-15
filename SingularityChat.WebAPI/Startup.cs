using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SingularityChat.WebAPI.Models;
using SingularityChat.WebAPI.Providers;
using SingularityChat.WebAPI.Services;
using SingularityChat.WebAPI.Utils;

namespace SingularityChat.WebAPI
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
            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseInMemoryDatabase("DB");
            });

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = "JWT";

            })
                .AddJwtBearer("JWT", opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = Configuration["JWT:Issuer"],
                        ValidAudience = Configuration["JWT:Audience"],
                        IssuerSigningKey = Configuration["JWT:PrivateKey"].GetSecurityKey(),
                    };

                    opt.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/gateway")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("Test", new AuthorizationPolicyBuilder()
                    .RequireClaim("OcChoType")
                    .RequireClaim("DoNgu")
                    .Build());
            });

            services.AddControllers();

            services.AddJwtService(opt => opt
                .WithPrivateKey(Configuration["JWT:PrivateKey"])
                .WithIssuer(Configuration["JWT:Issuer"])
                .WithAudience(Configuration["JWT:Audience"]));

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSignalR();

            services.AddSingleton<IUserIdProvider, UserIdProvider>();

            services.AddSingleton<ChatRoomServices>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseRouting();
            app.UseCors(opt =>
            {
                opt.WithOrigins("http://localhost:3000");
                opt.AllowAnyMethod();
                opt.AllowAnyHeader();
                opt.AllowCredentials();
            });
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/gateway");
            });

            app.ApplicationServices.GetRequiredService<ChatRoomServices>();
        }
    }
}
