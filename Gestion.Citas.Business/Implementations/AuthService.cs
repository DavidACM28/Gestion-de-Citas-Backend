using Gestion.Citas.Business.DTO.Request.Auth;
using Gestion.Citas.Business.DTO.Response.Auth;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.Repositories.Interfaces;

namespace Gestion.Citas.Business.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        public AuthService(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
        {
            //Validacion de credenciales
            var user = await _userRepository.GetByPredicateAsync(p => p.Username == request.UserName);
            if (user is null)
                return Result.Failure<LoginResponse>("Usuario no existe");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Result.Failure<LoginResponse>("Contraseña incorrecta");

            //Generacion de Token
            var result = _tokenService.Generate(user);

            var response = new LoginResponse
            {
                Token = result.Value!.Token,
                ExpirationDate = result.Value!.ExpirationDate,
                Role = user.Role
            };

            return Result.Success(response);
        }
    }
}
