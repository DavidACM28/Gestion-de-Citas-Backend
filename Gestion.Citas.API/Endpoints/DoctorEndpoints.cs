using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.Doctor;
using Gestion.Citas.Business.DTO.Response.Doctor;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;
using System.Security.Claims;

namespace Gestion.Citas.API.Endpoints
{
    public static class DoctorEndpoints
    {
        public static RouteGroupBuilder MapDoctorEndpoints(this RouteGroupBuilder group)
        {
            group.MapPost("/", async (CreateDoctorRequest request, IDoctorService service) =>
            {
                var result = await service.RegisterAsync(request);
                if (result.IsFailure)
                    return Results.BadRequest(result);
                return Results.Created("api/doctors", result);
            })
                .WithName("CreateDoctor")
                .WithSummary("Crea un doctor")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin))
                .Produces<CreateDoctorResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/me", async (ClaimsPrincipal currentUser, IDoctorService service) =>
            {
                var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var result = await service.GetMeAsync(userId);
                if (result.IsFailure)
                    return Results.NotFound(result);
                return Results.Ok(result);
            })
                .WithName("GetCurrentDoctor")
                .WithSummary("Obtiene la información del doctor autenticado")
                .RequireAuthorization(d => d.RequireRole(Roles.Doctor, Roles.Admin))
                .Produces<CreateDoctorResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
