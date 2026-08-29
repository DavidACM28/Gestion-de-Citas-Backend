using Gestion.Citas.Business.DTO.Request.AppointmentBlock;
using Gestion.Citas.Business.DTO.Response.AppointmentBlock;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Implementations;
using Gestion.Citas.Repositories.Interfaces;
using Mapster;

namespace Gestion.Citas.Business.Implementations
{
    public class AppointmentBlockService : IAppointmentBlockService
    {
        private readonly IAppointmentBlockRepository _appointmentBlockRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        public AppointmentBlockService(IAppointmentBlockRepository appointmentBlockRepository, IAppointmentRepository appointmentRepository, IDoctorRepository doctorRepository)
        {
            _appointmentBlockRepository = appointmentBlockRepository;
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
        }
        public async Task<Result<CreateAppointmentBlockResponse>> CreateAsync(CreateAppointmentBlockRequest request, int userId, string role)
        {
            //validaciones del request
            var blockStartDateTime = new DateTime(request.Date, request.StartTime);
            if (request.EndTime <= request.StartTime)
                return Result.Failure<CreateAppointmentBlockResponse>( "La hora de fin debe ser mayor a la hora de inicio");
            if (blockStartDateTime < DateTime.UtcNow.AddHours(-5)) 
            {
                return Result.Failure<CreateAppointmentBlockResponse>("El bloqueo no puede ser creado en el pasado");
            }
            if (role.Equals("Doctor"))
            {
                var requestDoctor = await _doctorRepository.GetByUserIdAsync(userId);
                if (requestDoctor.IsFailure)
                    return Result.Failure<CreateAppointmentBlockResponse>("No se encontró al doctor");
                if(requestDoctor.Value!.Id != request.DoctorId)
                {
                    return Result.Failure<CreateAppointmentBlockResponse>("No se puede crear un bloqueo para otro doctor");
                }
            }
            //validación de existencia del doctor
            var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
            if (doctor == null)
            {
                return Result.Failure<CreateAppointmentBlockResponse>("No se encontró al doctor");
            }

            //validación de colisión de bloqueo con otros bloqueos
            var blockExists = await _appointmentBlockRepository.GetByPredicateAsync(b =>
                b.Active && b.DoctorId == request.DoctorId && b.Date == request.Date && 
                b.StartTime < request.EndTime && b.EndTime > request.StartTime);

            if (blockExists is not null)
                return Result.Failure<CreateAppointmentBlockResponse>("Ya existe un bloqueo que colisiona con el horario indicado");

            //validación de colisión de bloqueo con citas existentes
            var appointments = await _appointmentRepository.ListInRange(request.Date, request.StartTime, request.EndTime, request.DoctorId);
            if(appointments is null || appointments.Count <= 0)
            {
                var result = await _appointmentBlockRepository.CreateAsync(request.Adapt<AppointmentBlock>());
                return Result.Success(result.Adapt<CreateAppointmentBlockResponse>());
            }
            if (!request.Force)
                return Result.Failure<CreateAppointmentBlockResponse>("Hay citas que no permiten crear el bloqueo");
            if(role.Equals("Doctor"))
                return Result.Failure<CreateAppointmentBlockResponse>("Hay citas que no permiten crear el bloqueo");
            var resultForce = await _appointmentBlockRepository.ForceCreateAsync(request.Adapt<AppointmentBlock>(), appointments);
            if (resultForce.IsFailure || resultForce.Value is null)
                return Result.Failure<CreateAppointmentBlockResponse>("No se pudo crear el bloqueo");
            return Result.Success(resultForce.Value.Adapt<CreateAppointmentBlockResponse>());
        }

        public async Task<Result<List<GetAppointmentBlockResponse>>> GetByFiltersAsync(int doctorId, DateOnly? startDate, DateOnly? endDate, int userId, string role)
        {
            if (startDate is not null && endDate is not null && startDate > endDate)
                return Result.Failure<List<GetAppointmentBlockResponse>>("La fecha de inicio no puede ser mayor que la fecha de fin");

            var result = await _appointmentBlockRepository.GetByFiltersAsync(doctorId, startDate, endDate, userId, role);
            if (result.IsFailure)
                return Result.Failure<List<GetAppointmentBlockResponse>>(result.Message!);

            List<GetAppointmentBlockResponse> blocks = [];
            foreach(var block in result.Value!)
            {
                blocks.Add(block.Adapt<GetAppointmentBlockResponse>());
            }
            return Result.Success(blocks);
        }

        public async Task<Result> DeleteAsync(int id, int userId, string role)
        {
            var block = await _appointmentBlockRepository.GetByIdAsync(id);
            if (block is null)
                return Result.Failure("Bloqueo no encontrado");

            if (role.Equals("Doctor"))
            {
                var doctor = await _doctorRepository.GetByUserIdAsync(userId);
                if (doctor.IsFailure || doctor.Value is null || doctor.Value.Id != block.DoctorId)
                    return Result.Failure("No se puede eliminar el bloqueo de otro doctor");
            }

            await _appointmentBlockRepository.DeleteAsync(id);
            return Result.Success();
        }
    }
}
