using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.User;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;

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

            return group;
        }
    }
}
