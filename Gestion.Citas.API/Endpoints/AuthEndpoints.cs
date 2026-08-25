using Gestion.Citas.Business.DTO.Request.Auth;
using Gestion.Citas.Business.DTO.Response.Auth;
using Gestion.Citas.Business.Interfaces;

namespace Gestion.Citas.API.Endpoints
{
    public static class AuthEndpoints
    {
        public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
        {
            group.MapPost("/login", async (LoginRequest request, IAuthService service) =>
            {
                var result = await service.LoginAsync(request);
                if (result.IsFailure)
                    return Results.BadRequest(result);
                return Results.Ok(result);
            })
                .WithName("Login")
                .WithSummary("Autentica y devuelve el JWT (HS250, 60 min)")
                .Produces<LoginResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }
    }
}
