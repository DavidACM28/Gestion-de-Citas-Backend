using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gestion.Citas.Repositories.Implementations
{
    public class AppointmentBlockRepository(AppointmentsDbContext context) : BaseRepository<AppointmentBlock>(context), IAppointmentBlockRepository
    {
        public async Task<Result<AppointmentBlock>> ForceCreateAsync(AppointmentBlock appointmentBlock, List<Appointment> appointments)
        {
            using (var trx = await _context.Database.BeginTransactionAsync())
            {
                try 
                {
                    appointmentBlock = await CreateAsync(appointmentBlock);
                    foreach (var appointment in appointments)
                    {
                        appointment.Status = "CANCELED";
                        appointment.Note = "Cancelada por bloqueo de agenda del doctor";
                        await _context.Set<AppointmentSlot>()
                            .Where(s => s.AppointmentId == appointment.Id)
                            .ExecuteDeleteAsync();
                    }
                    await _context.SaveChangesAsync();
                    await trx.CommitAsync();
                    return Result.Success(appointmentBlock);
                }
                catch (Exception) 
                {
                    await trx.RollbackAsync();
                    return Result.Failure<AppointmentBlock>("No se pudo crear el bloqueo y cancelar las citas");
                }

            }
        }

        public async Task<Result<List<AppointmentBlock>>> GetByFiltersAsync(int doctorId, DateOnly? startDate, DateOnly? endDate, int userId, string role)
        {
            var query = _context.Set<AppointmentBlock>()
                .Where(b => b.Active)
                .AsQueryable();

            if (role.Equals("Doctor"))
            {
                var doctor = await _context.Set<Doctor>()
                    .FirstOrDefaultAsync(d => d.Active && d.UserId == userId);
                if (doctor is null)
                    return Result.Failure<List<AppointmentBlock>>("Doctor no encontrado");

                query = query.Where(b => b.DoctorId == doctor.Id);
            }
            else if (doctorId != 0)
            {
                query = query.Where(b => b.DoctorId == doctorId);
            }

            if (startDate is not null)
                query = query.Where(b => b.Date >= startDate.Value);
            if (endDate is not null)
                query = query.Where(b => b.Date <= endDate.Value);

            var result = await query
                .OrderBy(b => b.Date)
                .ThenBy(b => b.StartTime)
                .ToListAsync();

            return Result.Success(result);
        }
    }
}
