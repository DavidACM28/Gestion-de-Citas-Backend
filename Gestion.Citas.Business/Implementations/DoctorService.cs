using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.Doctor;
using Gestion.Citas.Business.DTO.Response.Doctor;
using Gestion.Citas.Business.DTO.Response.Specialty;
using Gestion.Citas.Business.DTO.Response.User;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Gestion.Citas.Business.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISpecialtyRepository _specialtyRepository;
        public DoctorService(IDoctorRepository doctorRepository, IUserRepository userRepository, ISpecialtyRepository specialtyRepository)
        {
            _doctorRepository = doctorRepository;
            _userRepository = userRepository;
            _specialtyRepository = specialtyRepository;
        }

        public async Task<Result> RegisterAsync(CreateDoctorRequest request)
        {
            //Validacion de la peticion
            if (string.IsNullOrWhiteSpace(request.Username))
                return Result.Failure("El nombre de usuario es obligatorio");
            if (string.IsNullOrWhiteSpace(request.Email))
                return Result.Failure("El email es obligatorio");
            if (string.IsNullOrWhiteSpace(request.Password))
                return Result.Failure("La contraseña es obligatoria");
            if (string.IsNullOrWhiteSpace(request.FirstName))
                return Result.Failure("El nombre es obligatorio");
            if (string.IsNullOrWhiteSpace(request.LastName))
                return Result.Failure("El apellido es obligatorio");
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                return Result.Failure("El numero de telefono es obligatorio");

            //Validación de existencia de usuario
            var userNameexists = await _userRepository.UserExistsAsync(request.Username);
            if (userNameexists)
                return Result.Failure($"El nombre de usuario {request.Username} ya existe");

            //Validación de existencia por email
            var userEmailExists = await _userRepository.GetByPredicateAsync(d => d.Email == request.Email);
            if (userEmailExists != null)
                return Result.Failure($"El email {request.Email} ya está registrado");

            //Validación de existencia de especialidad
            var specialtyExists = await _specialtyRepository.GetByIdAsync(request.SpecialtyId);
            if (specialtyExists == null)
                return Result.Failure($"La especialidad con el id: {request.SpecialtyId} no existe");

            //Crear las instancias de Doctor y User
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Roles.Doctor
            };

            var doctor = request.Adapt<Doctor>();
            var result = await _doctorRepository.CreateWithUserAsync(doctor, user);

            //Validar si el proceso fue exitoso y crear la respuesta
            if (result.IsFailure)
                return result;

            doctor = result.Value;
            var response = new CreateDoctorResponse
            {
                Id = doctor!.Id,
                FirstName = doctor!.FirstName,
                LastName = doctor!.LastName,
                PhoneNumber = doctor!.PhoneNumber,
                Specialty = new GetSpecialtyResponse
                {
                    Id = specialtyExists.Id,
                    Name = specialtyExists.Name,
                    Description = specialtyExists.Description
                },
                User = new GetUserResponse
                {
                    Id = doctor.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                }
            };

            return Result.Success(response);
        }

        public async Task<Result<GetDoctorResponse>> GetMeAsync(int userId)
        {
            var result = await _doctorRepository.GetByUserIdAsync(userId);
            if (result.IsFailure || result.Value is null || result.Value.User is null || result.Value.Specialty is null)
                return Result.Failure<GetDoctorResponse>("Doctor no encontrado");

            return Result.Success(new GetDoctorResponse
            {
                Id = result.Value.Id,
                FirstName = result.Value.FirstName,
                LastName = result.Value.LastName,
                PhoneNumber = result.Value.PhoneNumber,
                User = result.Value.User.Adapt<GetUserResponse>(),
                Specialty = result.Value.Specialty.Adapt<GetSpecialtyResponse>()
            });
        }
    }
}
