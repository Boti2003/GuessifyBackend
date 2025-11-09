using GuessifyBackend.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GuessifyBackend.Feature
{
    public static class UserFeature
    {
        private record RequestReg(string Username, string Email, string Password);

        public static void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/registerWithUserName", async (RequestReg request, UserManager<User> userManager) =>
            {
                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    DisplayName = request.Username
                };
                var result = await userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    return Results.Ok(new { Message = "User registered successfully." });
                }
                else
                {
                    return Results.BadRequest(result.Errors);
                }
            });

            app.MapGet("/user/me", async (ClaimsPrincipal claimsPrincipal, UserManager<User> userManager) =>
            {
                Console.WriteLine("SZOPJ LE");
                var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Results.Unauthorized();
                }
                var user = await userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return Results.NotFound();
                }


                return Results.Ok(new
                {
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Id = user.Id
                }
                );
            }).RequireAuthorization();
        }
    }
}
