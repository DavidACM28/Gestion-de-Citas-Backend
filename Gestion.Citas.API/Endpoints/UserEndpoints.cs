using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.User;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;
using System.Security.Claims;

namespace Gestion.Citas.API.Endpoints
{
    public static class UserEndpoints
    {
        public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
        {
            group.MapPost("/", async (CreateUserRequest request, IUserService service) =>
            {
                var result = await service.RegisterAsync(request);
                if (result.IsFailure)
                    return Results.BadRequest(result);
                return Results.Created("api/user", result);
            })
                .WithName("User Registration")
                .WithSummary("Registrar un nuevo usuario de tipo Admin o Receptionitst")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin))
                .Produces<Result>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/", async (IUserService service) =>
            {
                var result = await service.ListAsync();
                if(result.IsFailure || result.Value is null)
                    return Results.NotFound(result);
                return Results.Ok(result);
            })
                .WithName("Get List Users")
                .WithSummary("Listado de usuarios")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin))
                .Produces<Result>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/{id:int}", async (int id, IUserService service) =>
            {
                var result = await service.GetByIdAsync(id);
                if (result.IsFailure | result.Value is null)
                    return Results.NotFound(result);
                return Results.Ok(result);
            })
                .WithName("Get User by Id")
                .WithSummary("Detalle de un usuario")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin))
                .Produces<Result>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/me", async (ClaimsPrincipal currentUser, IUserService service) =>
            {
                var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var result = await service.GetMeAsync(userId);
                if (result.IsFailure)
                    return Results.NotFound(result);

                return Results.Ok(result);
            })
                   .WithName("GetCurrentUser")
                   .WithSummary("Obtiene la información del usuario autenticado")
                   .RequireAuthorization(p => p.RequireRole(Roles.Receptionist, Roles.Admin))
                   .Produces<Result>(StatusCodes.Status200OK)
                   .Produces(StatusCodes.Status401Unauthorized)
                   .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
