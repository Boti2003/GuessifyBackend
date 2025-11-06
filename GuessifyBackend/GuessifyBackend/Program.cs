
using GuessifyBackend.Entities;
using GuessifyBackend.Hubs;
using GuessifyBackend.Service;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace GuessifyBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);



            // Add services to the container.
            builder.Services.AddDbContext<GameDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<DeezerApiService>();
            builder.Services.AddScoped<MusicDbSetupService>();
            builder.Services.AddSingleton<LobbyService>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<GameService>();
            builder.Services.AddScoped<QuestionService>();
            builder.Services.AddSingleton<GameEventManager>();
            builder.Services.AddSingleton<VotingService>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            /*var setup = await SetupConfigService.ParseConfigData("setup_game.json");
            var dbSetupService = new MusicDbSetupService(builder.Services.BuildServiceProvider().GetRequiredService<GameDbContext>(), builder.Services.BuildServiceProvider().GetRequiredService<DeezerApiService>());
            await dbSetupService.BuildMusicSbStructure(setup);*/

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
                /*app.UseSwagger();
                app.UseSwaggerUI();*/
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors();

            app.MapControllers();
            app.MapHub<LobbyHub>("/lobbyhub");
            app.MapHub<GameHub>("/gamehub");

            app.Run();
        }
    }
}
