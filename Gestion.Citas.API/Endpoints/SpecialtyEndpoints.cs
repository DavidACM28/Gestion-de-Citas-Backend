using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.Specialty;
using Gestion.Citas.Business.DTO.Response.Specialty;
using Gestion.Citas.Business.Interfaces;
using System.Security.Permissions;

namespace Gestion.Citas.API.Endpoints
{
    public static class SpecialtyEndpoints
    {
        public static RouteGroupBuilder MapSpecialtyEndpoints(this RouteGroupBuilder group)
        {
            group.MapPost("/", async (CreateSpecialtyRequest request, ISpecialtyService service) =>
            {
                var result = await service.CreateAsync(request);
                if (result.IsFailure)
                    return Results.BadRequest(result);
                return Results.Created("api/specialties", result);
            })
                .WithName("CreateSpecialty")
                .WithSummary("Crea una especialidad medica")
                .RequireAuthorization(p => p.RequireRole(Roles.Admin))
                .Produces<CreateSpecialtyResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/", async (int pageNumber, int pageSize, ISpecialtyService service) =>
            {
                var specialties = await service.ListAsync(pageNumber, pageSize);
                if (specialties == null)
                    return Results.NotFound();
                return Results.Ok(specialties);
            })
                .WithName("GetListSpecialties")
                .WithSummary("Listado de especialidades")
                .RequireAuthorization()
                .Produces<List<GetSpecialtyResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/{id:int}", async (int id, ISpecialtyService specialtyService) =>
            {
                var specialty = await specialtyService.GetByIdAsync(id);
                if (specialty.IsFailure)
                    return Results.NotFound(specialty);
                return Results.Ok(specialty);
            })
                .WithName("GetSpecialtyById")
                .WithSummary("Detalle de una especialidad")
                .RequireAuthorization()
                .Produces<GetSpecialtyResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
