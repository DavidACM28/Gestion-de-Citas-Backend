using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.Doctor;
using Gestion.Citas.Business.DTO.Response.Doctor;
using Gestion.Citas.Business.Interfaces;

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

            return group;
        }
    }
}
