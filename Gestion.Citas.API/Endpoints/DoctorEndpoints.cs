using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.Doctor;
using Gestion.Citas.Business.DTO.Response.Doctor;
using Gestion.Citas.Business.DTO.Response.BusinessHours;
using Gestion.Citas.Business.DTO.Response.Appointment;
using Gestion.Citas.Business.Interfaces;
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

            group.MapGet("/", async (string specialty, string name, int pageNumber, int pageSize, ClaimsPrincipal currentUser, IDoctorService service) =>
            {
                var role = currentUser.FindFirstValue(ClaimTypes.Role);
                var result = await service.GetByFilters(specialty, name, pageNumber, pageSize, role!);
                if(result.IsFailure || result.Value == null || result.Value.Count <= 0)
                {
                    return Results.NotFound(result);
                }
                return Results.Ok(result);
            })
                .WithName("GetDoctorsByFilters")
                .WithSummary("Obtiene la información de doctores por filtros")
                .RequireAuthorization(d => d.RequireRole(Roles.Doctor, Roles.Admin, Roles.Patient, Roles.Receptionist))
                .Produces<GetDoctorResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/{id:int}",  async (int id, IDoctorService service) =>
            {
                var result = await service.GetByIdAsync(id);
                if(result.IsFailure || result.Value is null)
                    return Results.NotFound(result);
                return Results.Ok(result);
            })
                .WithName("GetDoctorById")
                .WithSummary("Obtiene la información de doctores por Id")
                .RequireAuthorization(d => d.RequireRole(Roles.Admin, Roles.Receptionist))
                .Produces<GetDoctorResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/me/appointments", async (
                ClaimsPrincipal currentUser,
                IAppointmentService service,
                DateOnly? startDate = null,
                DateOnly? endDate = null,
                string status = "",
                int pageNumber = 1,
                int pageSize = 10) =>
            {
                if (!int.TryParse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Results.Unauthorized();

                var result = await service.GetByFiltersAsync(
                    role: Roles.Doctor,
                    userId: userId,
                    startDate: startDate,
                    endDate: endDate,
                    status: status,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                if (result.IsFailure)
                {
                    if (result.Message == "La fecha de inicio de la busqueda no puede ser mayor a la fecha de fin de la busqueda" ||
                        result.Message == "Estado de cita inválido")
                        return Results.BadRequest(result);
                    return Results.NotFound(result);
                }

                return Results.Ok(result);
            })
                .WithName("GetCurrentDoctorAppointments")
                .WithSummary("Obtiene las citas del doctor autenticado por fecha y estado")
                .RequireAuthorization(d => d.RequireRole(Roles.Doctor))
                .Produces<List<GetAppointmentResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/{doctorId:int}/business-hours", async (int doctorId, IBusinessHoursService service) =>
            {
                var result = await service.GetByDoctorIdAsync(doctorId);
                if (result.IsFailure)
                    return Results.NotFound(result);

                return Results.Ok(result);
            })
                .WithName("GetDoctorBusinessHours")
                .WithSummary("Obtiene los horarios de un doctor por su Id")
                .RequireAuthorization(d => d.RequireRole(Roles.Admin, Roles.Doctor, Roles.Patient, Roles.Receptionist))
                .Produces<List<GetBusinessHoursResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

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
                .Produces<GetDoctorResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
