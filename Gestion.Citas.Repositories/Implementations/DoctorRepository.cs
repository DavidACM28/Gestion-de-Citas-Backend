using Azure.Core;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

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
                    return Result.Failure<Doctor>($"Error al crear doctor y usuario: {ex.Message}");
                }
            }
        }

        public async Task<Result<Doctor>> GetByIdWithUserAndSpecialtyAsync(int id)
        {
            var doctor = await _context.Set<Doctor>()
                .Include(p => p.User)
                .Include(p => p.Specialty)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (doctor is null)
                return Result.Failure<Doctor>("No se encontraron resultados");
            return Result.Success(doctor);
        }

        public async Task<Result<List<Doctor>>> GetByFiltersAsync(string specialty, string name, int pageNumber, int pageSize, string role)
        {
            var query = _context.Set<Doctor>()
                .Include(d => d.Specialty)
                .Include(d => d.User)
                .AsQueryable();
            if (role == "Patient")
                query = query.Where(d => d.Active);

            if (!string.IsNullOrWhiteSpace(specialty))
                query = query.Where(d => d.Specialty!.Name.Equals(specialty));

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(d => d.FirstName.Equals(name) || d.LastName == name);

            var result = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (result.Count <= 0)
            {
                return Result.Failure<List<Doctor>>("No se encontraron resultados");
            }

            return Result.Success(result);
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
