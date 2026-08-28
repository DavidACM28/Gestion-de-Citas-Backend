using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.Appointment;
using Gestion.Citas.Business.DTO.Response.Appointment;
using Gestion.Citas.Business.DTO.Response.Doctor;
using Gestion.Citas.Business.Interfaces;
using System.Security.Claims;

namespace Gestion.Citas.API.Endpoints
{
    public static class AppointmentEndpoints
    {
        public static RouteGroupBuilder MapAppointmentEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (
                                IAppointmentService service, ClaimsPrincipal currentUser, string doctorFirstName = "", string doctorLastName = "", 
                                string patientFirstName = "", string patientLastName = "", string specialtyName ="", string status = "", int pageNumber = 1, 
                                int pageSize = 10, DateOnly? startDate = null, DateOnly? endDate = null,
                                int doctorId = 0, int patientId = 0, int specialtyId = 0)
                                =>
            {
                var role = currentUser.FindFirstValue(ClaimTypes.Role);
                var userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await service.GetByFiltersAsync(
                    doctorId: doctorId,
                    doctorFirstName: doctorFirstName,
                    doctorLastName: doctorLastName,
                    patientId: patientId,
                    patientFirstName: patientFirstName,
                    patientLastName: patientLastName,
                    specialtyId: specialtyId,
                    specialtyName: specialtyName,
                    startDate: startDate,
                    endDate: endDate,
                    status: status,
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    role: role!,
                    userId: userId
                    );
                if (result.IsFailure)
                {
                    if(result.Message!.Equals("La fecha de inicio de la busqueda no puede ser mayor a la fecha de fin de la busqueda"))
                        return Results.BadRequest(result);
                    if(result.Message!.Equals("Estado de cita inválido"))
                        return Results.BadRequest(result);
                    return Results.NotFound(result);
                }
                return Results.Ok(result);
            })
                .WithName("GetAppointmentsByFilters")
                .WithSummary("Obtiene la información de citas por filtros")
                .RequireAuthorization(d => d.RequireRole(Roles.Admin, Roles.Patient, Roles.Receptionist))
                .Produces<GetDoctorResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/", async (CreateAppointmentRequest request, IAppointmentService service, ClaimsPrincipal currentUser) =>
            {
                var userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = currentUser.FindFirstValue(ClaimTypes.Role);
                var result = await service.CreateAsync(request, userId, userRole!);
                if (result.IsFailure)
                    return Results.BadRequest(result);
                return Results.Created("api/appointments", result);
            })
                .WithName("CreateAppointment")
                .WithSummary("Crea una cita")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Receptionist, Roles.Patient))
                .Produces<CreateAppointmentResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapPut("/{id:int}", async (int id, UpdateAppointmentRequest request, IAppointmentService service, ClaimsPrincipal currentUser) =>
            {
                if (!int.TryParse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Results.Unauthorized();

                var userRole = currentUser.FindFirstValue(ClaimTypes.Role);
                var result = await service.UpdateAsync(request, id, userId, userRole!);
                if (result.IsFailure)
                {
                    if (result.Message!.Equals("Cita no encontrada"))
                        return Results.NotFound(result);
                    if (result.Message.Equals("No se pueden modificar citas de otros pacientes"))
                        return Results.Forbid();
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
                .WithName("UpdateAppointment")
                .WithSummary("Actualiza una cita y sus slots")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Receptionist, Roles.Patient))
                .Produces<UpdateAppointmentResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPatch("/{id:int}/confirm", async (int id, IAppointmentService service) =>
            {
                var result = await service.ConfirmAsync(id);
                if (result.IsFailure)
                    if (result.Message!.Equals("Cita no encontrada"))
                        return Results.NotFound(result);
                    else
                        return Results.BadRequest(result);
                return Results.Ok(result);
            })
                .WithName("ConfirmAppointment")
                .WithSummary("Confirma una cita")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Receptionist))
                .Produces<UpdateAppointmentResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPatch("/{id:int}/start", async (int id, IAppointmentService service) =>
            {
                var result = await service.StartAsync(id);
                if (result.IsFailure)
                    if (result.Message!.Equals("Cita no encontrada"))
                        return Results.NotFound(result);
                    else
                        return Results.BadRequest(result);
                return Results.Ok(result);
            })
                .WithName("StartAppointment")
                .WithSummary("Inicia una cita")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Receptionist))
                .Produces<UpdateAppointmentResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPatch("/{id:int}/finish", async (int id, IAppointmentService service) =>
            {
                var result = await service.FinishAsync(id);
                if (result.IsFailure)
                    if (result.Message!.Equals("Cita no encontrada"))
                        return Results.NotFound(result);
                    else
                        return Results.BadRequest(result);
                return Results.Ok(result);
            })
                .WithName("FinishAppointment")
                .WithSummary("Finaliza una cita")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Receptionist))
                .Produces<UpdateAppointmentResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPatch("/{id:int}/did-not-attend", async (int id, IAppointmentService service) =>
            {
                var result = await service.DidNotAttendAsync(id);
                if (result.IsFailure)
                    if (result.Message!.Equals("Cita no encontrada"))
                        return Results.NotFound(result);
                    else
                        return Results.BadRequest(result);
                return Results.Ok(result);
            })
                .WithName("DidNotAttendAppointment")
                .WithSummary("Marca cita como no asistida")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Receptionist))
                .Produces<UpdateAppointmentResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPatch("/{id:int}/cancel", async (int id, IAppointmentService service, ClaimsPrincipal currentUser) =>
            {
                if (!int.TryParse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Results.Unauthorized();

                var userRole = currentUser.FindFirstValue(ClaimTypes.Role);

                var result = await service.CancelAsync(id, userId, userRole!);
                if (result.IsFailure)
                    if (result.Message!.Equals("Cita no encontrada"))
                        return Results.NotFound(result);
                    else
                        return Results.BadRequest(result);
                return Results.Ok(result);
            })
                .WithName("CancelAppointment")
                .WithSummary("Cancela una cita")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Receptionist, Roles.Patient))
                .Produces<UpdateAppointmentResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
