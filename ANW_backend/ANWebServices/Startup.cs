using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Repositories;
using ANWebServices.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using ANWebServices.Interfaces;
using ANWebServices.API;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.Http;
using ANWebServices.Environment;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Amnhac.Api;

namespace ANWebServices
{
    public class Startup
    {
        public static string secret_key = "";
        private static string OriginAllowed = "";
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string SecretKey= Configuration.GetSection("Crypto:secret_key").Value;
            Startup.secret_key = Configuration.GetSection("Crypto:sha_key").Value;
            CharUtil.Key = Configuration.GetSection("AesCrypto:key").Value;
            CharUtil.Salt = Configuration.GetSection("AesCrypto:salt").Value;
            SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
            // Database Config
            services.Configure<DbSettings>(opt =>
            {
                opt.ConnectionString = Configuration.GetSection("MongoConnection:ConnectionString").Value;
                opt.Database = Configuration.GetSection("MongoConnection:Database").Value;
            });
            // Cross Origin - Shared Resources
            #region Cross Origin Configuration
            OriginAllowed = Configuration.GetSection("AccessControl:Origins").Value;
            string[] Origins = OriginAllowed.Replace(" ","").Split(",");
            services.AddCors(option =>
            {
                option.AddPolicy("ServeablePages", builder => builder
                .WithOrigins(Origins)
                .AllowAnyMethod()
                .AllowCredentials()
                .AllowAnyHeader());
            });
            #endregion
            // Add Database
            #region Data Access Service
            services.AddScoped<IRepository<Nation>, NationDAO>();
            services.AddScoped<IRepository<Slider>, SliderDAO>();
            services.AddScoped<IRepository<Album>, AlbumDAO>();
            services.AddScoped<IRepository<Song>, SongDAO>();
            services.AddScoped<IRepository<Artist>, ArtistDAO>();
            services.AddScoped<IRepository<SongType>, TypeDAO>();
            services.AddSingleton<IRepository<Permission>, PermissionDAO>();
            services.AddSingleton<IJwtFactory, JwtFactory>();
            services.AddTransient<IRepository<User>, UserDAO>();
            services.AddTransient<IRepository<Video>, VideoDAO>();
            services.AddTransient<IRepository<Playlist>, PlaylistDAO>();
            services.AddTransient<IRepository<SongRanked>, SongRankDAO>();
            services.AddTransient<IRepository<RankingCategory>, RankingCategoryDAO>();
            services.AddSingleton<IRepository<SystemConfig>, SysConfigDAO>();
            services.AddSingleton<IRepository<SongLogs>, SongLogsDAO>();
            services.AddSingleton<IRepository<Room>, RoomDAO>();
            services.AddSingleton<IRepository<UserConnection>, UserConnectionContext>();
            services.AddHostedService<LogsHandler>();
            #endregion
            // Jwt Section
            #region JwtConfiguration
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtOptions));

            services.Configure<JwtOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });
            // Add Token Validator
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = true,
                ValidateLifetime = true
            };
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy(nameof(UserRole.ADMIN), policy => 
                    policy.RequireClaim(UserAuthenticationSettings.ClaimRole, 
                    UserAuthenticationSettings.AdministratorRole));
            });

            services.AddAuthentication(options => {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = tokenValidationParameters;
                o.IncludeErrorDetails = true;

                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/signalr")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            #endregion


            services.AddMvc();

            services.AddSignalR().AddMessagePackProtocol();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
            }
            #region Error Handler
            app.UseExceptionHandler(
                builder =>
                {
                    builder.Run(
                        async context =>
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                                var error = context.Features.Get<IExceptionHandlerFeature>();
                                if (error != null)
                                {
                                    await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                                }
                            });
                });
            #endregion
            app.UseAuthentication();
            app.UseCors("ServeablePages");
            app.UseMvc();
            #region StaticFiles and Caching settings
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse= http =>
                {
                    // Cache configuration
                    const int age =60*60*24*30;
                    string path = http.Context.Request.Path;
                    if (path.EndsWith(".png") || path.EndsWith(".jpg") || 
                        path.EndsWith(".jpeg") || path.EndsWith(".bmp") ||
                        path.EndsWith(".svg") || path.EndsWith(".gif") ||
                        path.EndsWith(".ico")
                    )
                    http.Context.Response.Headers[HeaderNames.CacheControl]
                    = "public,max-age=" + age.ToString();
                }
            }
            );
            #endregion
            #region SignalR
            app.UseSignalR(route =>
            {
                route.MapHub<SignalRService>("/signalr");
            });
            #endregion
        }
    }
}
