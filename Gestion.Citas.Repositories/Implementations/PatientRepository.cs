using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gestion.Citas.Repositories.Implementations
{
    public class PatientRepository(AppointmentsDbContext context) : BaseRepository<Patient>(context), IPatientRepository
    {
        public async Task<Result<Patient>> CreateWithUserAsync(Patient patient, User user)
        {
            using (var trx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await _context.Set<User>().AddAsync(user);
                    await _context.SaveChangesAsync();
                    patient.UserId = result.Entity.Id;

                    var patientResult = await CreateAsync(patient);
                    await trx.CommitAsync();

                    return Result.Success(patientResult);
                }
                catch (Exception ex)
                {
                    await trx.RollbackAsync();
                    return (Result<Patient>)Result.Failure($"Error al crear paciente y usuario: {ex.Message}");
                }
            }
        }

        public async Task<Result<Patient>> GetByUserIdAsync(int userId)
        {
            var patient = await _context.Set<Patient>()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Active && p.UserId == userId);
            if (patient is null)
                return Result.Failure<Patient>("Paciente no encontrado");
            return Result.Success(patient);
        }

        public async Task<Result<Patient>> GetWithUserByIdAsync(int id)
        {
            var patient = await _context.Set<Patient>()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (patient is null)
                return Result.Failure<Patient>("Paciente no encontrado");
            return Result.Success(patient);
        }
    }
}
