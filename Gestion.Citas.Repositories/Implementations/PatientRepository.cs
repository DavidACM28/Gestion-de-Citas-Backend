using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
