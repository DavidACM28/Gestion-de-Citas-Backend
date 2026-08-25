using Gestion.Citas.Business.DTO.Request.Patient;
using Gestion.Citas.Business.DTO.Response.Patient;
using Gestion.Citas.Business.Interfaces;

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

            return group;
        }
    }
}

