using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.BusinessHours;
using Gestion.Citas.Business.DTO.Response.BusinessHours;
using Gestion.Citas.Business.DTO.Response.Doctor;
using Gestion.Citas.Business.DTO.Response.Specialty;
using Gestion.Citas.Business.DTO.Response.User;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Mapster;

namespace Gestion.Citas.Business.Implementations
{
    public class BusinessHoursService : IBusinessHoursService
    {
        private readonly IBusinessHoursRepository _businessHoursRepository;
        private readonly IDoctorRepository _doctorRepository;
        public BusinessHoursService(IBusinessHoursRepository businessHoursRepository, IDoctorRepository doctorRepository)
        {
            _businessHoursRepository = businessHoursRepository;
            _doctorRepository = doctorRepository;
        }

        public async Task<Result<CreateBusinessHoursResponse>> CreateAsync(CreateBusinessHoursRequest request)
        {
            //Validacion de la peticion
            if (string.IsNullOrWhiteSpace(request.DayOfWeek.ToString()))
                return Result.Failure<CreateBusinessHoursResponse>("El dia es obligatorio");
            if (string.IsNullOrWhiteSpace(request.StartTime.ToString()))
                return Result.Failure<CreateBusinessHoursResponse>("La hora de inicio es obligatoria");
            if (string.IsNullOrWhiteSpace(request.EndTime.ToString()))
                return Result.Failure<CreateBusinessHoursResponse>("La hora de fin es obligatoria");
            if (!request.DayOfWeek.Equals(DayOfWeek.Monday) &&
                !request.DayOfWeek.Equals(DayOfWeek.Tuesday) &&
                !request.DayOfWeek.Equals(DayOfWeek.Wednesday) &&
                !request.DayOfWeek.Equals(DayOfWeek.Thursday) &&
                !request.DayOfWeek.Equals(DayOfWeek.Friday) &&
                !request.DayOfWeek.Equals(DayOfWeek.Saturday) &&
                !request.DayOfWeek.Equals(DayOfWeek.Sunday))
                return Result.Failure<CreateBusinessHoursResponse>("Día de la semana inválido");

            //Validacion de existencia de doctor
            var doctor = await _doctorRepository.GetByIdWithUserAndSpecialtyAsync(request.DoctorId);
            if (doctor is null)
                return Result.Failure<CreateBusinessHoursResponse>("Doctor no encontrado");
            var dayExists = await _businessHoursRepository.GetByPredicateAsync(p => p.DayOfWeek == request.DayOfWeek && p.DoctorId == request.DoctorId);
            if (dayExists != null)
                return Result.Failure<CreateBusinessHoursResponse>($"Ya existe el horario del día {request.DayOfWeek} para el doctor con id {request.DoctorId}");

            var result = await _businessHoursRepository.CreateAsync(request.Adapt<BusinessHours>());
            var response = new CreateBusinessHoursResponse
            {
                id = result.Id,
                DayOfWeek = result.DayOfWeek,
                AppointmentDurationMin = result.AppointmentDurationMin,
                StartTime = result.StartTime,
                EndTime = result.EndTime,
                Doctor = new GetDoctorResponse
                {
                    Id = doctor.Value!.Id,
                    FirstName = doctor.Value!.FirstName,
                    LastName = doctor.Value!.LastName,
                    PhoneNumber = doctor.Value!.PhoneNumber,
                    Specialty = doctor.Value.Specialty!.Adapt<GetSpecialtyResponse>(),
                    User = doctor.Value.User!.Adapt<GetUserResponse>()
                }
            };
            return Result.Success(response);
        }

        public async Task<Result<GetBusinessHoursResponse>> GetByIdAsync(int id)
        {
            var result = await _businessHoursRepository.GetByIdWithDoctorAsync(id);
            if (result.IsFailure || result.Value?.Doctor is null || result.Value.Doctor.Specialty is null)
                return Result.Failure<GetBusinessHoursResponse>("Horario no encontrado");

            return Result.Success(ToResponse(result.Value));
        }

        public async Task<Result<GetBusinessHoursResponse>> UpdateAsync(int id, UpdateBusinessHoursRequest request, int userId, string role)
        {
            var result = await _businessHoursRepository.GetByIdWithDoctorAsync(id);
            if (result.IsFailure || result.Value?.Doctor is null)
                return Result.Failure<GetBusinessHoursResponse>("Horario no encontrado");

            var businessHours = result.Value;
            if (role == Roles.Doctor && businessHours.Doctor.UserId != userId)
                return Result.Failure<GetBusinessHoursResponse>("No tienes permiso para editar este horario");

            var duplicatedDay = await _businessHoursRepository.GetByPredicateAsync(h =>
                h.Active && h.Id != id && h.DoctorId == businessHours.DoctorId && h.DayOfWeek == request.DayOfWeek);
            if (duplicatedDay is not null)
                return Result.Failure<GetBusinessHoursResponse>("Ya existe un horario para ese día");

            businessHours.DayOfWeek = request.DayOfWeek;
            businessHours.StartTime = request.StartTime;
            businessHours.EndTime = request.EndTime;
            businessHours.AppointmentDurationMin = request.AppointmentDurationMin;

            await _businessHoursRepository.UpdateAsync();
            return Result.Success(ToResponse(businessHours));
        }

        public async Task<Result> DeleteAsync(int id)
        {
            var result = await _businessHoursRepository.GetByIdAsync(id);
            if (result is null)
                return Result.Failure("Horario no encontrado");

            await _businessHoursRepository.DeleteAsync(id);
            return Result.Success("Horario eliminado correctamente");
        }

        private static GetBusinessHoursResponse ToResponse(BusinessHours businessHours)
        {
            return new GetBusinessHoursResponse
            {
                Id = businessHours.Id,
                DayOfWeek = businessHours.DayOfWeek,
                StartTime = businessHours.StartTime,
                EndTime = businessHours.EndTime,
                AppointmentDurationMin = businessHours.AppointmentDurationMin,
                DoctorName = $"{businessHours.Doctor!.FirstName} {businessHours.Doctor.LastName}",
                SpecialtyName = businessHours.Doctor.Specialty?.Name ?? string.Empty,
                SpecialtyDescription = businessHours.Doctor.Specialty?.Description ?? string.Empty
            };
        }

        public async Task<Result<List<GetBusinessHoursResponse>>> GetByDoctorIdAsync(int doctorId)
        {
            var doctor = await _doctorRepository.GetByIdWithUserAndSpecialtyAsync(doctorId);
            if (doctor.IsFailure || doctor.Value is null)
                return Result.Failure<List<GetBusinessHoursResponse>>("Doctor no encontrado");

            var schedules = await _businessHoursRepository.GetByDoctorIdAsync(doctorId);
            var response = schedules.Value!
                .Select(schedule => new GetBusinessHoursResponse
                {
                    Id = schedule.Id,
                    DayOfWeek = schedule.DayOfWeek,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                    AppointmentDurationMin = schedule.AppointmentDurationMin,
                    DoctorName = $"{doctor.Value.FirstName} {doctor.Value.LastName}",
                    SpecialtyName = doctor.Value.Specialty?.Name ?? string.Empty,
                    SpecialtyDescription = doctor.Value.Specialty?.Description ?? string.Empty
                })
                .ToList();

            return Result.Success(response);
        }
    }
}
