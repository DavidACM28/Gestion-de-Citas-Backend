using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.Patient;
using Gestion.Citas.Business.DTO.Response.Patient;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;
using System.Security.Claims;

namespace Gestion.Citas.API.Endpoints
{
    public static class PatientEndpoints
    {
        public static RouteGroupBuilder MapPatientEndpoints(this RouteGroupBuilder group)
        {
            group.MapPost("/", async (CreatePatientRequest request, IPatientService service) =>
            {
                var result = await service.RegisterAsync(request);
                if (result.IsFailure)
                    return Results.BadRequest(result);
                return Results.Created("api/patients", result);
            })
                .WithName("CreatePatient")
                .WithSummary("Crea un paciente")
                .Produces<CreatePatientResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/me", async (ClaimsPrincipal currentUser, IPatientService service) =>
            {
                var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var result = await service.GetMeAsync(userId);
                if (result.IsFailure)
                    return Results.NotFound(result);
                return Results.Ok(result);
            })
                .WithName("GetCurrentPatient")
                .WithSummary("Obtiene la información del paciente autenticado")
                .RequireAuthorization(p => p.RequireRole(Roles.Patient, Roles.Admin))
                .Produces<GetPatientResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}

