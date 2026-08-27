using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.BusinessHours;
using Gestion.Citas.Business.DTO.Response.BusinessHours;
using Gestion.Citas.Business.DTO.Response.Doctor;
using Gestion.Citas.Business.Interfaces;
using System.Security.Claims;

namespace Gestion.Citas.API.Endpoints
{
    public static class BusinessHoursEndpoints
    {
        public static RouteGroupBuilder MapBusinessHoursEndpoints(this RouteGroupBuilder group)
        {
            group.MapPost("/", async (CreateBusinessHoursRequest request, IBusinessHoursService service) =>
            {
                var result = await service.CreateAsync(request);
                if (result.IsFailure || result.Value is null)
                    return Results.BadRequest(result);
                return Results.Created("api/businessHours", result);
            })

                .WithName("CreateBusinessHours")
                .WithSummary("Crea el horario de atención de un día")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin))
                .Produces<CreateDoctorResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/{id:int}", async (int id, IBusinessHoursService service) =>
            {
                var result = await service.GetByIdAsync(id);
                if (result.IsFailure)
                    return Results.NotFound(result);

                return Results.Ok(result);
            })
                .WithName("GetBusinessHoursById")
                .WithSummary("Obtiene un horario por Id")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin))
                .Produces<GetBusinessHoursResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPut("/{id:int}", async (int id, UpdateBusinessHoursRequest request, ClaimsPrincipal currentUser, IBusinessHoursService service) =>
            {
                var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var role = currentUser.FindFirstValue(ClaimTypes.Role);
                var result = await service.UpdateAsync(id, request, userId, role!);
                if (result.IsFailure)
                {
                    if (result.Message!.Equals("Horario no encontrado"))
                        return Results.NotFound(result);
                    if (result.Message.Equals("No tienes permiso para editar este horario"))
                        return Results.Forbid();
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
                .WithName("UpdateBusinessHours")
                .WithSummary("Edita un horario")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Doctor))
                .Produces<GetBusinessHoursResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete("/{id:int}", async (int id, IBusinessHoursService service) =>
            {
                var result = await service.DeleteAsync(id);
                if (result.IsFailure)
                    return Results.NotFound(result);

                return Results.Ok(result);
            })
                .WithName("DeleteBusinessHours")
                .WithSummary("Desactiva un horario")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin))
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }

    }
}
