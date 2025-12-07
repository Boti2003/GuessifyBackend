
using GuessifyBackend.Entities;
using GuessifyBackend.Entities.Identity;
using GuessifyBackend.Hubs;
using GuessifyBackend.Jobs;
using GuessifyBackend.Service.Implementations;
using GuessifyBackend.Service.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

namespace GuessifyBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);



            // Add services to the container.
            builder.Services.AddDbContext<GameDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IDeezerApiService, DeezerApiService>();
            builder.Services.AddScoped<IMusicDbSetupService, MusicDbSetupService>();
            builder.Services.AddScoped<ISetupConfigService, SetupConfigService>();
            builder.Services.AddSingleton<ILobbyService, LobbyService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ITokenProviderService, TokenProviderService>();
            builder.Services.AddScoped<IGameService, GameService>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddSingleton<IGameEventManager, GameEventManager>();
            builder.Services.AddSingleton<IVotingService, VotingService>();
            builder.Services.AddProblemDetails();
            builder.Host.UseSerilog();
            builder.Services.AddQuartz(q =>
            {
                // Just use the name of your job that you created in the Jobs folder.
                var jobKey = new JobKey("SetupMusicDb");
                q.AddJob<MusicDbSetupJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("SetupMusicDb-trigger")
                    .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Saturday, 22, 2))
                );
            });
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            //builder.Services.AddControllers();
            builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.PropertyNamingPolicy = null;
                        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                    });

            builder.Services.AddIdentityCore<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            /*builder.Services.AddAuthentication()
                .AddBearerToken(IdentityConstants.BearerScheme);*/


            builder.Services.AddAuthentication(options =>
            {
                // Identity made Cookie authentication the default.
                // However, we want JWT Bearer Auth to be the default.
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                /*options.DefaultAuthenticateScheme = BearerTokenDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = BearerTokenDefaults.AuthenticationScheme;*/
            }).
            AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // Configure the Authority to the expected value for
                // the authentication provider. This ensures the token
                // is appropriately validated.


                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {

                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
                };

                // We have to hook the OnMessageReceived event in order to
                // allow the JWT authentication handler to read the access
                // token from the query string when a WebSocket or 
                // Server-Sent Events request comes in.

                // Sending the access token in the query string is required when using WebSockets or ServerSentEvents
                // due to a limitation in Browser APIs. We restrict it to only calls to the
                // SignalR hub in this code.
                // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
                // for more information about security considerations when using
                // the query string to transmit the access token.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/lobbyHub") || path.StartsWithSegments("/gameHub")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:5173")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
            });

            builder.Services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.Converters
                   .Add(new JsonStringEnumConverter());
            }); ;

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

            }
            /*app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    var exceptionHandlerPathFeature =
                        context.Features.Get<IExceptionHandlerPathFeature>();
                    Console.WriteLine(exceptionHandlerPathFeature?.Error);
                    if (exceptionHandlerPathFeature?.Error != null)
                    {
                        var errorResponse = new
                        {
                            Message = "An unexpected error occurred."

                        };
                        await context.Response.WriteAsJsonAsync(errorResponse);
                    }
                });
            });*/

            /*app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async httpContext =>
                {
                    var pds = httpContext.RequestServices.GetService<IProblemDetailsService>();

                    if (pds == null
                        || !await pds.TryWriteAsync(new() { HttpContext = httpContext }))
                    {

                        await httpContext.Response.WriteAsync("Fallback: An error occurred.");
                    }
                });
            });*/
            app.UseCors();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();



            app.MapControllers();


            app.MapHub<LobbyHub>("/lobbyhub");
            app.MapHub<GameHub>("/gamehub");

            app.Run();
        }
    }
}
