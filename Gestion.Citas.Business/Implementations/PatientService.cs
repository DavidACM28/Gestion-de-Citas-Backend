using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.Patient;
using Gestion.Citas.Business.DTO.Response.Patient;
using Gestion.Citas.Business.DTO.Response.User;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Mapster;

namespace Gestion.Citas.Business.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IUserRepository _userRepository;

        public PatientService(IPatientRepository patientRepository, IUserRepository userRepository)
        {
            _patientRepository = patientRepository;
            _userRepository = userRepository;
        }

        public async Task<Result> RegisterAsync(CreatePatientRequest request)
        {
            //Validacion de la peticion
            if (string.IsNullOrWhiteSpace(request.Username))
                return Result.Failure("El nombre de usuario es obligatorio");
            if (string.IsNullOrWhiteSpace(request.Email))
                return Result.Failure("El email es obligatorio");
            if (string.IsNullOrWhiteSpace(request.Password))
                return Result.Failure("La contraseña es obligatoria");
            if (string.IsNullOrWhiteSpace(request.DocumentType))
                return Result.Failure("El tipo de documento es obligatorio");
            if (string.IsNullOrWhiteSpace(request.DocumentNumber))
                return Result.Failure("El número de documento es obligatorio");
            if (string.IsNullOrWhiteSpace(request.FirstName))
                return Result.Failure("El nombre es obligatorio");
            if (string.IsNullOrWhiteSpace(request.LastName))
                return Result.Failure("El apellido es obligatorio");
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                return Result.Failure("El numero de telefono es obligatorio");
            if (!request.DocumentType.Equals(Constants.DocumentTypes.DNI) && 
                !request.DocumentType.Equals(Constants.DocumentTypes.CE) && 
                !request.DocumentType.Equals(Constants.DocumentTypes.Passport)
                )
                return Result.Failure("El tipo de documento no es válido, use: DNI o CE o Passport");

            //Validación de existencia de usuario
            var userNameexists = await _userRepository.UserExistsAsync(request.Username);
            if (userNameexists)
                return Result.Failure($"El nombre de usuario {request.Username} ya existe");

            //Validación de existencia por email
            var userEmailExists = await _userRepository.GetByPredicateAsync(p => p.Email == request.Email);
            if (userEmailExists != null)
                return Result.Failure($"El email {request.Email} ya está registrado");

            //Validación de existencia por numero de documento
            var userDocumentExists = await _patientRepository.GetByPredicateAsync(p => p.DocumentNumber == request.DocumentNumber);
            if (userDocumentExists != null)
                return Result.Failure($"El documento {request.DocumentNumber} ya está registrado");

            //Crear las instancias de Patient y User
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Roles.Patient
            };

            var patient = request.Adapt<Patient>();
            var result = await _patientRepository.CreateWithUserAsync(patient, user);

            if (result.IsFailure)
                return result;

            patient = result.Value;

            var response = new CreatePatientResponse
            {
                Id = patient!.Id,
                DocumentType = patient!.DocumentType,
                DocumentNumber = patient!.DocumentNumber,
                FirstName = patient!.FirstName,
                LastName = patient!.LastName,
                DateOfBirth = patient!.DateOfBirth,
                PhoneNumber = patient!.PhoneNumber,
                Address = patient!.Address,
                User = new GetUserResponse
                {
                    Id = patient.UserId,
                    Email = request.Email,
                    Username = request.Username,
                    Role = Roles.Patient
                }
            };
            return Result.Success(response);
        }

        public async Task<Result<GetPatientResponse>> GetMeAsync(int userId)
        {
            var result = await _patientRepository.GetByUserIdAsync(userId);
            if (result.IsFailure || result.Value is null || result.Value.User is null)
                return Result.Failure<GetPatientResponse>("Paciente no encontrado");

            return Result.Success(new GetPatientResponse
            {
                Id = result.Value.Id,
                DocumentType = result.Value.DocumentType,
                DocumentNumber = result.Value.DocumentNumber,
                FirstName = result.Value.FirstName,
                LastName = result.Value.LastName,
                DateOfBirth = result.Value.DateOfBirth,
                PhoneNumber = result.Value.PhoneNumber,
                Address = result.Value.Address,
                User = result.Value.User.Adapt<GetUserResponse>()
            });
        }
    }
}
