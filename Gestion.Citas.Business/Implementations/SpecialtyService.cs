using Gestion.Citas.Business.DTO.Request.Specialty;
using Gestion.Citas.Business.DTO.Response.Specialty;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Mapster;

namespace Gestion.Citas.Business.Implementations
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly ISpecialtyRepository _specialtyRepository;
        public SpecialtyService(ISpecialtyRepository repository)
        {
            _specialtyRepository = repository;
        }

        public async Task<Result> CreateAsync(CreateSpecialtyRequest request)
        {
            //Validaciones de request
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result.Failure("El nombre es obligatorio");

            if (string.IsNullOrWhiteSpace(request.Description))
                return Result.Failure("La descripcion es obligatoria");

            var specialtyNameExists = await _specialtyRepository.GetByPredicateAsync(p => p.Name == request.Name);
            if (specialtyNameExists != null)
                return Result.Failure($"La especialidad {request.Name} ya existe");

            var specialty = new Specialty
            {
                Name = request.Name,
                Description = request.Description
            };

            specialty = await _specialtyRepository.CreateAsync(specialty);
            var response = specialty.Adapt<CreateSpecialtyResponse>();

            return Result.Success<CreateSpecialtyResponse>(response);
        }

        public async Task<Result<GetSpecialtyResponse>> GetByIdAsync(int id)
        {
            var result = await _specialtyRepository.GetByIdAsync(id);
            if (result is null)
                return Result.Failure<GetSpecialtyResponse>("Especialidad no encontrada");
            return Result.Success(result.Adapt<GetSpecialtyResponse>());
        }

        public async Task<Result<List<GetSpecialtyResponse>>> ListAsync(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _specialtyRepository.ListAsync(
                predicate: s => s.Active,
                selector: s => new GetSpecialtyResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description
                },
                pageNumber: pageNumber,
                pageSize: pageSize
                );
            return Result.Success(result.Result.ToList());
        }
    }
}
