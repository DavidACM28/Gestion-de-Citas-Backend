using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Repositories.Implementations
{
    public class BusinessHoursRepository(AppointmentsDbContext context) : BaseRepository<BusinessHours>(context), IBusinessHoursRepository
    {
        public async Task<Result<List<BusinessHours>>> GetByDoctorIdAsync(int doctorId)
        {
            var result = await _context.Set<BusinessHours>()
                .Where(h => h.Active && h.DoctorId == doctorId)
                .OrderBy(h => h.DayOfWeek)
                .ThenBy(h => h.StartTime)
                .ToListAsync();

            return Result.Success(result);
        }

        public async Task<Result<BusinessHours>> GetByIdWithDoctorAsync(int id)
        {
            var result = await _context.Set<BusinessHours>()
                .Include(h => h.Doctor!)
                    .ThenInclude(d => d.Specialty)
                .FirstOrDefaultAsync(h => h.Active && h.Id == id);

            if (result is null)
                return Result.Failure<BusinessHours>("Horario no encontrado");
            return Result.Success(result);
        }

    }
}
