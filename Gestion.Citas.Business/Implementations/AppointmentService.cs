using Gestion.Citas.Business.Constants;
using Gestion.Citas.Business.DTO.Request.Appointment;
using Gestion.Citas.Business.DTO.Response.Appointment;
using Gestion.Citas.Business.DTO.Response.Doctor;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Mapster;

namespace Gestion.Citas.Business.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IBusinessHoursRepository _businessHoursRepository;
        private readonly IAppointmentBlockRepository _appointmentBlockRepository;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IBusinessHoursRepository businessHoursRepository,
            IAppointmentBlockRepository appointmentBlockRepository
            )
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _businessHoursRepository = businessHoursRepository;
            _appointmentBlockRepository = appointmentBlockRepository;
        }

        public async Task<Result<CreateAppointmentResponse>> CreateAsync(CreateAppointmentRequest request, int userId, string userRole)
        {
            //Validaciones de request
            if (string.IsNullOrWhiteSpace(request.Date.ToString()))
                return Result.Failure<CreateAppointmentResponse>("La fecha es obligatoria");
            if (string.IsNullOrWhiteSpace(request.StartTime.ToString()))
                return Result.Failure<CreateAppointmentResponse>("La hora de inicio es obligatoria");
            if (string.IsNullOrWhiteSpace(request.Reason))
                return Result.Failure<CreateAppointmentResponse>("La razón es obligatoria");
            DateTime appointmentDateTime = new DateTime(request.Date, request.StartTime);
            if (appointmentDateTime < DateTime.UtcNow.AddHours(-5))
                return Result.Failure<CreateAppointmentResponse>("No se puede agendar una cita con una fecha menor al tiempo actual");

            //Validacion de existencia de doctor
            var doctorExists = await _doctorRepository.GetByIdWithUserAndSpecialtyAsync(request.DoctorId);
            if (doctorExists.IsFailure || doctorExists.Value is null)
                return Result.Failure<CreateAppointmentResponse>("Doctor no encontrado");

            //Validacion de existencia de paciente y que la cita sea para el propio paciente
            var patientExists = await _patientRepository.GetWithUserByIdAsync(request.PatientId);
            if (patientExists.IsFailure || patientExists.Value is null)
                return Result.Failure<CreateAppointmentResponse>("Paciente no encontrado");
            if (userRole.Equals(Roles.Patient) && patientExists.Value.User!.Id != userId)
                return Result.Failure<CreateAppointmentResponse>("No se pueden solicitar citas para otros pacientes");

            //Validación de hora en rango de trabajo de doctor
            DayOfWeek appointmentDay = request.Date.DayOfWeek;
            var doctorBusinessHours = await _businessHoursRepository
                .GetByPredicateAsync(b => b.Active && b.DoctorId == request.DoctorId && b.DayOfWeek == appointmentDay);
            if (doctorBusinessHours is null)
                return Result.Failure<CreateAppointmentResponse>("El doctor no tiene horario disponible en el día indicado");
            TimeOnly endTime = request.StartTime.AddMinutes(doctorBusinessHours.AppointmentDurationMin);
            if (request.StartTime < doctorBusinessHours.StartTime || endTime > doctorBusinessHours.EndTime)
                return Result.Failure<CreateAppointmentResponse>("El doctor no tiene disponibilidad en este horario");

            //Validacion de bloqueo de citas en rango de cita
            var blocks = await _appointmentBlockRepository
                .GetByPredicateAsync(
                    b => b.Active && b.DoctorId == request.DoctorId &&
                    b.Date == request.Date && b.StartTime < endTime && b.EndTime > request.StartTime);
            if (blocks is not null)
                return Result.Failure<CreateAppointmentResponse>("El doctor no tiene disponibilidad en este horario");

            //Calcular los slots que va a ocupar la cita, se calculan de forma predeterminada en slots de 15 minutos
            List<AppointmentSlot> slots = [];
            TimeOnly startSlot = new(request.StartTime.Hour, (request.StartTime.Minute / 15) * 15);

            for (TimeOnly time = startSlot; time < endTime; time = time.AddMinutes(15))
            {
                slots.Add(new AppointmentSlot
                {
                    Doctor = doctorExists.Value,
                    DoctorId = doctorExists.Value.Id,
                    Date = request.Date,
                    Time = time
                });
            }

            var result = await _appointmentRepository.CreateWithSlotsAsync(
                appointment: new Appointment
                {
                    Date = request.Date,
                    Doctor = doctorExists.Value,
                    DoctorId = doctorExists.Value.Id,
                    DurationMin = doctorBusinessHours.AppointmentDurationMin,
                    Patient = patientExists.Value,
                    PatientId = patientExists.Value.Id,
                    Note = request.Note,
                    Reason = request.Reason,
                    StartTime = request.StartTime,
                    Status = AppointmentStatus.REQUESTED
                },
                slots: slots);
            if (result.IsFailure || result.Value is null)
                return Result.Failure<CreateAppointmentResponse>(result.Message!);
            return Result.Success(new CreateAppointmentResponse
            {
                Id = result.Value.Id,
                DoctorId = result.Value.DoctorId,
                DoctorName = $"{doctorExists.Value.FirstName} {doctorExists.Value.LastName}",
                Specialty = doctorExists.Value.Specialty!.Name,
                PatientId = patientExists.Value.Id,
                PatientName = $"{patientExists.Value.FirstName} {patientExists.Value.LastName}",
                PatientDocumentType = patientExists.Value.DocumentType,
                PatientDocumentNumber = patientExists.Value.DocumentNumber,
                PatientPhoneNumber = patientExists.Value.PhoneNumber,
                Date = result.Value.Date,
                StartTime = result.Value.StartTime,
                EndTime = endTime,
                DurationMin = doctorBusinessHours.AppointmentDurationMin,
                Reason = result.Value.Reason,
                Note = result.Value.Note,
                Status = result.Value.Status
            });
        }

        public async Task<Result<UpdateAppointmentResponse>> UpdateAsync(UpdateAppointmentRequest request, int id, int userId, string userRole)
        {
            if (string.IsNullOrWhiteSpace(request.Date.ToString()))
                return Result.Failure<UpdateAppointmentResponse>("La fecha es obligatoria");
            if (string.IsNullOrWhiteSpace(request.StartTime.ToString()))
                return Result.Failure<UpdateAppointmentResponse>("La hora de inicio es obligatoria");
            if (string.IsNullOrWhiteSpace(request.Reason))
                return Result.Failure<UpdateAppointmentResponse>("La razón es obligatoria");
            DateTime appointmentDateTime = new DateTime(request.Date, request.StartTime);
            if (appointmentDateTime < DateTime.UtcNow.AddHours(-5))
                return Result.Failure<UpdateAppointmentResponse>("No se puede agendar una cita con una fecha menor al tiempo actual");

            var appointmentResult = await _appointmentRepository.GetByIdWithDetailsAsync(id);
            if (appointmentResult.IsFailure || appointmentResult.Value is null ||
                appointmentResult.Value.Doctor is null || appointmentResult.Value.Patient is null)
                return Result.Failure<UpdateAppointmentResponse>("Cita no encontrada");

            var appointment = appointmentResult.Value;
            if (!appointment.Status.Equals(AppointmentStatus.REQUESTED))
                return Result.Failure<UpdateAppointmentResponse>($"Solo se pueden editar las citas con estado {AppointmentStatus.REQUESTED}");
            if (userRole.Equals(Roles.Patient) && appointment.Patient.User!.Id != userId)
                return Result.Failure<UpdateAppointmentResponse>("No se pueden modificar citas de otros pacientes");

            var doctorExists = await _doctorRepository.GetByIdWithUserAndSpecialtyAsync(request.DoctorId);
            if (doctorExists.IsFailure || doctorExists.Value is null)
                return Result.Failure<UpdateAppointmentResponse>("Doctor no encontrado");

            var patientExists = await _patientRepository.GetWithUserByIdAsync(request.PatientId);
            if (patientExists.IsFailure || patientExists.Value is null)
                return Result.Failure<UpdateAppointmentResponse>("Paciente no encontrado");
            if (userRole.Equals(Roles.Patient) && patientExists.Value.User!.Id != userId)
                return Result.Failure<UpdateAppointmentResponse>("La cita no se puede cambiar de paciente");

            var businessHours = await _businessHoursRepository.GetByPredicateAsync(b =>
                b.Active && b.DoctorId == request.DoctorId && b.DayOfWeek == request.Date.DayOfWeek);
            if (businessHours is null)
                return Result.Failure<UpdateAppointmentResponse>("El doctor no tiene horario disponible en el día indicado");

            var endTime = request.StartTime.AddMinutes(businessHours.AppointmentDurationMin);
            if (request.StartTime < businessHours.StartTime || endTime > businessHours.EndTime)
                return Result.Failure<UpdateAppointmentResponse>("El doctor no tiene disponibilidad en este horario");

            var blocks = await _appointmentBlockRepository.GetByPredicateAsync(b =>
                b.Active && b.DoctorId == request.DoctorId && b.Date == request.Date &&
                b.StartTime < endTime && b.EndTime > request.StartTime);
            if (blocks is not null)
                return Result.Failure<UpdateAppointmentResponse>("El doctor no tiene disponibilidad en este horario");

            var slotStart = new TimeOnly(request.StartTime.Hour, (request.StartTime.Minute / 15) * 15);
            List<AppointmentSlot> slots = [];
            for (var time = slotStart; time < endTime; time = time.AddMinutes(15))
            {
                slots.Add(new AppointmentSlot
                {
                    AppointmentId = appointment.Id,
                    DoctorId = request.DoctorId,
                    Date = request.Date,
                    Time = time
                });
            }

            appointment.Date = request.Date;
            appointment.StartTime = request.StartTime;
            appointment.DoctorId = request.DoctorId;
            appointment.Doctor = doctorExists.Value;
            appointment.PatientId = request.PatientId;
            appointment.Patient = patientExists.Value;
            appointment.DurationMin = businessHours.AppointmentDurationMin;
            appointment.Reason = request.Reason;
            appointment.Note = request.Note;

            var updateResult = await _appointmentRepository.UpdateWithSlotsAsync(appointment, slots);
            if (updateResult.IsFailure)
                return Result.Failure<UpdateAppointmentResponse>(updateResult.Message!);

            return Result.Success(new UpdateAppointmentResponse
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                DoctorName = $"{doctorExists.Value.FirstName} {doctorExists.Value.LastName}",
                Specialty = doctorExists.Value.Specialty!.Name,
                PatientId = appointment.PatientId,
                PatientName = $"{patientExists.Value.FirstName} {patientExists.Value.LastName}",
                PatientDocumentType = patientExists.Value.DocumentType,
                PatientDocumentNumber = patientExists.Value.DocumentNumber,
                PatientPhoneNumber = patientExists.Value.PhoneNumber,
                Date = appointment.Date,
                StartTime = appointment.StartTime,
                EndTime = endTime,
                DurationMin = appointment.DurationMin,
                Status = appointment.Status,
                Reason = appointment.Reason,
                Note = appointment.Note
            });

        }

        public async Task<Result<List<GetAppointmentResponse>>> GetByFiltersAsync(
            string role,
            int userId,
            int doctorId = 0,
            string doctorFirstName = "",
            string doctorLastName = "",
            int patientId = 0,
            string patientFirstName = "",
            string patientLastName = "",
            int specialtyId = 0,
            string specialtyName = "",
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            string status = "",
            int pageNumber = 1,
            int pageSize = 10
            )
        {
            if (startDate != null && endDate != null)
            {
                if (startDate > endDate)
                    return Result.Failure<List<GetAppointmentResponse>>("La fecha de inicio de la busqueda no puede ser mayor a la fecha de fin de la busqueda");
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!string.Equals(status, AppointmentStatus.REQUESTED, StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(status, AppointmentStatus.CONFIRMED, StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(status, AppointmentStatus.BEING_ATTENDED, StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(status, AppointmentStatus.FINISHED, StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(status, AppointmentStatus.CANCELED, StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(status, AppointmentStatus.DID_NOT_ATTEND, StringComparison.OrdinalIgnoreCase)
                    )
                    return Result.Failure<List<GetAppointmentResponse>>("Estado de cita inválido");
            }
            var result = await _appointmentRepository.GetByFiltersAsync(
                doctorId: doctorId,
                doctorFirstName: doctorFirstName,
                doctorLastName: doctorLastName,
                patientId: patientId,
                patientFirstName: patientFirstName,
                patientLastName: patientLastName,
                specialtyId: specialtyId,
                specialtyName: specialtyName,
                startDate: startDate,
                endDate: endDate,
                status: status,
                pageNumber: pageNumber,
                pageSize: pageSize,
                role: role,
                userId: userId
                );
            if (result.IsFailure)
                return Result.Failure<List<GetAppointmentResponse>>(result.Message!);
            List<GetAppointmentResponse> appointments = [];
            foreach(var appointment in result.Value!)
            {
                appointments.Add(new GetAppointmentResponse
                {
                    Id = appointment.Id,
                    DoctorId = appointment.DoctorId,
                    PatientId = appointment.PatientId,
                    PatientName = appointment.Patient is null ? string.Empty : $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
                    PatientFirstName = appointment.Patient?.FirstName ?? string.Empty,
                    PatientLastName = appointment.Patient?.LastName ?? string.Empty,
                    Date = appointment.Date,
                    StartTime = appointment.StartTime,
                    DurationMin = appointment.DurationMin,
                    Status = appointment.Status,
                    Reason = appointment.Reason,
                    Note = appointment.Note
                });
            }
            return Result.Success(appointments);
        }

        public async Task<Result<GetAppointmentResponse>> ConfirmAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment is null)
                return Result.Failure<GetAppointmentResponse>("Cita no encontrada");
            if (!appointment.Status.Equals(AppointmentStatus.REQUESTED))
                return Result.Failure<GetAppointmentResponse>($"Estado inválido, solo se puede confirmar una cita con estado {AppointmentStatus.REQUESTED}");
            appointment.Status = AppointmentStatus.CONFIRMED;
            await _appointmentRepository.UpdateAsync();
            return Result.Success(appointment.Adapt<GetAppointmentResponse>());
        }

        public async Task<Result<GetAppointmentResponse>> StartAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment is null)
                return Result.Failure<GetAppointmentResponse>("Cita no encontrada");
            if (!appointment.Status.Equals(AppointmentStatus.CONFIRMED))
                return Result.Failure<GetAppointmentResponse>($"Estado inválido, solo se puede confirmar una cita con estado {AppointmentStatus.REQUESTED}");
            appointment.Status = AppointmentStatus.BEING_ATTENDED;
            await _appointmentRepository.UpdateAsync();
            return Result.Success(appointment.Adapt<GetAppointmentResponse>());
        }

        public async Task<Result<GetAppointmentResponse>> FinishAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment is null)
                return Result.Failure<GetAppointmentResponse>("Cita no encontrada");
            if (!appointment.Status.Equals(AppointmentStatus.BEING_ATTENDED))
                return Result.Failure<GetAppointmentResponse>($"Estado inválido, solo se puede finalizar una cita con estado {AppointmentStatus.BEING_ATTENDED}");
            appointment.Status = AppointmentStatus.FINISHED;
            var result = await _appointmentRepository.DeleteAppointmentSlotsAsync(appointment);
            if (result.IsFailure || result.Value is null)
                return Result.Failure<GetAppointmentResponse>("No se pudo finalizar la cita");
            return Result.Success(result.Value.Adapt<GetAppointmentResponse>());
        }

        public async Task<Result<GetAppointmentResponse>> CancelAsync(int id, int userId, string userRole)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment is null)
                return Result.Failure<GetAppointmentResponse>("Cita no encontrada");
            if (!appointment.Status.Equals(AppointmentStatus.REQUESTED) && !appointment.Status.Equals(AppointmentStatus.CONFIRMED))
                return Result.Failure<GetAppointmentResponse>($"Estado inválido, solo se puede cancelar una cita con estado {AppointmentStatus.REQUESTED} o {AppointmentStatus.CONFIRMED}");
            if (userRole.Equals(Roles.Patient))
            {
                var patient = await _patientRepository.GetByUserIdAsync(userId);
                if (appointment.PatientId != patient.Value!.Id)
                    return Result.Failure<GetAppointmentResponse>("No puede cancelar una cita de otro paciente");
                DateTime appointmentDateTime = new DateTime(appointment.Date, appointment.StartTime);
                var diferencia = appointmentDateTime - (DateTime.UtcNow.AddHours(-5));
                if (diferencia.TotalHours < 2)
                    return Result.Failure<GetAppointmentResponse>("No se puede cancelar una cita con menos de 2 horas de anticipación");
            }
            appointment.Status = AppointmentStatus.CANCELED;
            var result = await _appointmentRepository.DeleteAppointmentSlotsAsync(appointment);
            if (result.IsFailure || result.Value is null)
                return Result.Failure<GetAppointmentResponse>("No se pudo cancelar la cita");
            return Result.Success(result.Value.Adapt<GetAppointmentResponse>());
        }

        public async Task<Result<GetAppointmentResponse>> DidNotAttendAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment is null)
                return Result.Failure<GetAppointmentResponse>("Cita no encontrada");
            if (!appointment.Status.Equals(AppointmentStatus.CONFIRMED))
                return Result.Failure<GetAppointmentResponse>($"Estado inválido, solo se puede marcar una cita como no asistida si el estado es {AppointmentStatus.CONFIRMED}");
            DateTime appointmentDateTime = new DateTime(appointment.Date, appointment.StartTime);
            var diferencia = (DateTime.UtcNow.AddHours(-5)) - appointmentDateTime;
            if (diferencia.Minutes < 10)
                return Result.Failure<GetAppointmentResponse>($"Deben pasar al menos 10 minutos para marcar la cita como no asistida");
            appointment.Status = AppointmentStatus.DID_NOT_ATTEND;
            var result = await _appointmentRepository.DeleteAppointmentSlotsAsync(appointment);
            if (result.IsFailure || result.Value is null)
                return Result.Failure<GetAppointmentResponse>("No se pudo marcar como no asistido");
            return Result.Success(result.Value.Adapt<GetAppointmentResponse>());
        }
    }
}
