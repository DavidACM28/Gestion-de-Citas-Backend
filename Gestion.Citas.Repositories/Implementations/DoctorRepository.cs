using Azure.Core;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Gestion.Citas.Repositories.Implementations
{
    public class DoctorRepository(AppointmentsDbContext context) : BaseRepository<Doctor>(context), IDoctorRepository
    {
        public async Task<Result<Doctor>> CreateWithUserAsync(Doctor doctor, User user)
        {
            using (var trx = await _context.Database.BeginTransactionAsync()) 
            {
                try
                {
                    var result = await _context.Set<User>().AddAsync(user);
                    await _context.SaveChangesAsync();
                    doctor.UserId = result.Entity.Id;

                    var doctorResult = await CreateAsync(doctor);
                    await trx.CommitAsync();

                    return Result.Success(doctorResult);
                }
                catch (Exception ex)
                {
                    await trx.RollbackAsync();
                    return (Result<Doctor>)Result.Failure($"Error al crear doctor y usuario: {ex.Message}");
                }
            }
        }

        public async Task<Result<Doctor>> GetByUserIdAsync(int userId)
        {
            var doctor = await _context.Set<Doctor>()
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.Active && d.UserId == userId);
            if (doctor is null)
                return (Result<Doctor>)Result.Failure("No se encontróp el doctor");
            return Result.Success(doctor);
        }
    }
}
