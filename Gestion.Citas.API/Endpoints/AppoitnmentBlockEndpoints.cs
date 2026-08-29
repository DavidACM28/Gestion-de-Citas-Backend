using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.AppointmentBlock;
using Gestion.Citas.Business.DTO.Response.Appointment;
using Gestion.Citas.Business.DTO.Response.AppointmentBlock;
using Gestion.Citas.Business.Interfaces;
using System.Security.Claims;

namespace Gestion.Citas.API.Endpoints
{
    public static class AppoitnmentBlockEndpoints
    {
        public static RouteGroupBuilder MapAppointmentBlockEndpoints(this RouteGroupBuilder group)
        {
            group.MapPost("/", async (CreateAppointmentBlockRequest request, IAppointmentBlockService service, ClaimsPrincipal currentUser) =>
            {
                var role = currentUser.FindFirstValue(ClaimTypes.Role);
                var userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await service.CreateAsync(request, userId, role!);
                if (result.IsFailure)
                    return Results.BadRequest(result);
                return Results.Created("api/appointmentBlocks", result);
            })
                .WithName("CreateAppointmentBlock")
                .WithSummary("Crea una bloqueo de cita")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Doctor))
                .Produces<CreateAppointmentBlockResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/", async (IAppointmentBlockService service, ClaimsPrincipal currentUser, int doctorId = 0,
                                DateOnly? startDate = null, DateOnly? endDate = null) 
                                =>
            {
                if (!int.TryParse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Results.Unauthorized();

                var role = currentUser.FindFirstValue(ClaimTypes.Role);
                var result = await service.GetByFiltersAsync(doctorId, startDate, endDate, userId, role!);
                if (result.IsFailure)
                {
                    if (result.Message == "La fecha de inicio no puede ser mayor que la fecha de fin")
                        return Results.BadRequest(result);
                    return Results.NotFound(result);
                }

                return Results.Ok(result);
            })
                .WithName("GetAppointmentBlocks")
                .WithSummary("Lista los bloqueos filtrados por doctor y fecha")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Doctor, Roles.Receptionist))
                .Produces<List<GetAppointmentBlockResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete("/{id:int}", async (int id, IAppointmentBlockService service, ClaimsPrincipal currentUser) =>
            {
                if (!int.TryParse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Results.Unauthorized();

                var role = currentUser.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
                var result = await service.DeleteAsync(id, userId, role);
                if (result.IsFailure)
                {
                    if (result.Message == "Bloqueo no encontrado")
                        return Results.NotFound(result);
                    if (result.Message == "No se puede eliminar el bloqueo de otro doctor")
                        return Results.Forbid();
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
                .WithName("DeleteAppointmentBlock")
                .WithSummary("Desactiva un bloqueo por Id")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Doctor))
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
