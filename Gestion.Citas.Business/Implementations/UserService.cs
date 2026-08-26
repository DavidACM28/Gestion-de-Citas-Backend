using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.User;
using Gestion.Citas.Business.DTO.Response.User;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Mapster;

namespace Gestion.Citas.Business.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<Result> RegisterAsync(CreateUserRequest request)
        {
            //Validaciones del request
            if (string.IsNullOrWhiteSpace(request.Username))
                return Result.Failure("El nombre de usuario es obligatorio");

            if (string.IsNullOrWhiteSpace(request.Password))
                return Result.Failure("La contraseña es obligatoria");

            if (string.IsNullOrWhiteSpace(request.Email))
                return Result.Failure("El email es obligatorio");

            if (string.IsNullOrWhiteSpace(request.Role))
                return Result.Failure("El rol es obligatorio");

            //Validación de existencia de usuario
            var usernameExists = await _userRepository.UserExistsAsync(request.Username);
            if (usernameExists)
                return Result.Failure($"El nombre de usuario {request.Username} ya existe");

            //Validación de existencia por email
            var userEmailExists = await _userRepository.GetByPredicateAsync(c => c.Email == request.Email);
            if (userEmailExists != null)
                return Result.Failure($"El email {request.Email} ya está registrado");

            //Validación de rol
            if (request.Role != Roles.Receptionist && request.Role != Roles.Admin)
                return Result.Failure($"El rol {request.Role} no es válido");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role
            };

            user = await _userRepository.CreateAsync(user);
            var response = user.Adapt<CreateUserResponse>();

            return Result.Success<CreateUserResponse>(response);
        }

        public async Task<Result<GetUserResponse>> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                return Result.Failure<GetUserResponse>("Usuario no encontrado");
            return Result.Success(user.Adapt<GetUserResponse>());
        }

        public async Task<Result<List<GetUserResponse>>> ListAsync(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _userRepository.ListAsync(
                predicate: p => 1 == 1,
                selector: p => new GetUserResponse
                {
                    Id = p.Id,
                    Username = p.Username,
                    Email = p.Email,
                    Role = p.Role
                },
                pageNumber: pageNumber,
                pageSize: pageSize
                );
            if (result.Result is null)
                return Result.Failure<List<GetUserResponse>>("No se encontraron resultados");
            return Result.Success(result.Result.ToList());
        }

        public async Task<Result<GetUserResponse>> GetMeAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                return Result.Failure<GetUserResponse>("Usuario no encontrado");

            return Result.Success(user.Adapt<GetUserResponse>());
        }
    }
}
