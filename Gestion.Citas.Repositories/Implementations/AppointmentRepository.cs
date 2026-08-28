using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gestion.Citas.Repositories.Implementations
{
    public class AppointmentRepository(AppointmentsDbContext context) : BaseRepository<Appointment>(context), IAppointmentRepository
    {
        public async Task<Result<Appointment>> CreateWithSlotsAsync(Appointment appointment, List<AppointmentSlot> slots)
        {
            using (var trx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await CreateAsync(appointment);
                    foreach (var slot in slots)
                    {
                        slot.AppointmentId = appointment.Id;
                    }
                    await _context.AddRangeAsync(slots);
                    await _context.SaveChangesAsync();
                    await trx.CommitAsync();

                    return Result.Success(appointment);
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException)
                {
                    await trx.RollbackAsync();
                    return Result.Failure<Appointment>($"Ya hay una cita en el rango de hora solicitado");
                }
                catch (Exception)
                {
                    await trx.RollbackAsync();
                    return Result.Failure<Appointment>($"No se pudo crear la cita");
                }
            }
        }
        public async Task<Result<Appointment>> GetByIdWithDetailsAsync(int id)
        {
            var appointment = await _context.Set<Appointment>()
                .Include(a => a.Doctor!)
                    .ThenInclude(d => d.Specialty)
                .Include(a => a.Patient!)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Active && a.Id == id);

            if (appointment is null)
                return Result.Failure<Appointment>("Cita no encontrada");
            return Result.Success(appointment);
        }
        public async Task<Result<List<Appointment>>> GetByFiltersAsync(
            int doctorId,
            string doctorFirstName,
            string doctorLastName,
            int patientId,
            string patientFirstName,
            string patientLastName,
            int specialtyId,
            string specialtyName,
            DateOnly? startDate,
            DateOnly? endDate,
            string status,
            int pageNumber,
            int pageSize,
            string role,
            int userId
            )
        {
            var query = _context.Set<Appointment>()
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.Specialty)
                .Include(a => a.Patient)
                .Where(a => a.Active)
                .AsQueryable();
            if (role.Equals("Patient"))
            {
                var patient = await _context.Set<Patient>().FirstOrDefaultAsync(p => p.Active && p.UserId == userId);
                if (patient is null)
                    return Result.Failure<List<Appointment>>("Paciente no encontrado");
                query = query.Where(a => a.PatientId == patient.Id);
            }
            if (role.Equals("Doctor"))
            {
                var doctor = await _context.Set<Doctor>().FirstOrDefaultAsync(d => d.Active && d.UserId == userId);
                if (doctor is null)
                    return Result.Failure<List<Appointment>>("Doctor no encontrado");
                query = query.Where(a => a.DoctorId == doctor.Id);
            }
            if(!doctorId.Equals(0))
                query = query.Where(a => a.DoctorId.Equals(doctorId));
            if (!string.IsNullOrWhiteSpace(doctorFirstName))
                query = query.Where(a => a.Doctor!.FirstName.Equals(doctorFirstName));
            if (!string.IsNullOrWhiteSpace(doctorLastName))
                query = query.Where(a => a.Doctor!.LastName.Equals(doctorLastName));
            if (!patientId.Equals(0))
                query = query.Where(a => a.PatientId.Equals(patientId));
            if (!string.IsNullOrWhiteSpace(patientFirstName))
                query = query.Where(a => a.Patient!.FirstName.Equals(patientFirstName));
            if (!string.IsNullOrWhiteSpace(patientLastName))
                query = query.Where(a => a.Patient!.LastName.Equals(patientLastName));
            if (!specialtyId.Equals(0))
                query = query.Where(a => a.Doctor!.SpecialtyId.Equals(specialtyId));
            if (!string.IsNullOrWhiteSpace(specialtyName))
                query = query.Where(a => a.Doctor!.Specialty!.Name.Equals(specialtyName));
            if (startDate != null && endDate != null)
                query = query.Where(a => a.Date >= startDate && a.Date<= endDate);
            if (startDate != null && endDate == null)
                query = query.Where(a => a.Date >= startDate);
            if (endDate != null && startDate == null)
                query = query.Where(a => a.Date <= endDate);
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(a => a.Status.Equals(status));
            
            var result = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (result.Count <= 0)
            {
                return Result.Failure<List<Appointment>>("No se encontraron resultados");
            }

            return Result.Success(result);
        }
        public async Task<bool> HasSlotConflictAsync(int doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int appointmentId)
        {
            return await _context.Set<AppointmentSlot>()
                .AnyAsync(s => s.DoctorId == doctorId && s.Date == date && s.AppointmentId != appointmentId &&
                    s.Time >= startTime && s.Time < endTime);
        }

        public async Task<Result<Appointment>> UpdateWithSlotsAsync(Appointment appointment, List<AppointmentSlot> slots)
        {
            using (var trx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.Set<AppointmentSlot>()
                        .Where(s => s.AppointmentId == appointment.Id)
                        .ExecuteDeleteAsync();

                    await _context.Set<AppointmentSlot>().AddRangeAsync(slots);
                    await _context.SaveChangesAsync();
                    await trx.CommitAsync();

                    return Result.Success(appointment);
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException)
                {
                    await trx.RollbackAsync();
                    return Result.Failure<Appointment>("Ya hay una cita en el rango de hora solicitado");
                }
                catch (Exception)
                {
                    await trx.RollbackAsync();
                    return Result.Failure<Appointment>("No se pudo actualizar la cita");
                }
            }

        }

        public async Task<Result<Appointment>> DeleteAppointmentSlotsAsync(Appointment appointment)
        {
            using (var trx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.Set<AppointmentSlot>()
                        .Where(s => s.AppointmentId == appointment.Id)
                        .ExecuteDeleteAsync();
                    await _context.SaveChangesAsync();
                    await trx.CommitAsync();
                    return Result.Success(appointment);
                }
                catch (Exception)
                {
                    await trx.RollbackAsync();
                    return Result.Failure<Appointment>("No se pudieron eliminar los slots");
                }
            }

        }
    }
}
