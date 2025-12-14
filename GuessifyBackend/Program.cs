
using GuessifyBackend.Entities;
using GuessifyBackend.Entities.Identity;
using GuessifyBackend.Handlers;
using GuessifyBackend.Hubs;
using GuessifyBackend.Jobs;
using GuessifyBackend.Service.Implementations;
using GuessifyBackend.Service.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
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

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(
                    "Logs/log.txt",
                   rollingInterval: RollingInterval.Day,
                   fileSizeLimitBytes: 10 * 1024 * 1024,
                   rollOnFileSizeLimit: true,
                   shared: true,
                   flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();
            Log.Information("Starting Guessify Server");


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
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddSerilog();
            builder.Services.AddQuartz(q =>
            {

                var jobKey = new JobKey("SetupMusicDb");
                q.AddJob<MusicDbSetupJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("SetupMusicDb-startup-trigger")
                    .StartNow()
                );

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("SetupMusicDb-trigger")
                    .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Monday, 3, 0))
                );
            });
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

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



            builder.Services.AddAuthentication(options =>
            {

                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).
            AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {



                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {

                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
                };


                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];


                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/lobbyHub") || path.StartsWithSegments("/gameHub")))
                        {

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
                        policy.WithOrigins("https://guessify.hu",
                            "https://guessify-blue.vercel.app",
                            "http://localhost:5173")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
            });

            builder.Services.AddSignalR().
            AddHubOptions<LobbyHub>(options =>
                options.AddFilter<ErrorHandlingLobbyHubFilter>()
            ).
            AddHubOptions<GameHub>(options =>
                options.AddFilter<ErrorHandlingGameHubFilter>()
            ).
            AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.Converters
                   .Add(new JsonStringEnumConverter());
            }); ;

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {

                    var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();
                    var gameDbContext = services.GetRequiredService<GameDbContext>();
                    gameDbContext.Database.Migrate();
                    applicationDbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Error during database migration.");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

            }

            app.UseExceptionHandler();
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
