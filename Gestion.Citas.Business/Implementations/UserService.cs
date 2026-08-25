using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.User;
using Gestion.Citas.Business.DTO.Response.User;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Implementations;
using Gestion.Citas.Repositories.Interfaces;
using Mapster;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
