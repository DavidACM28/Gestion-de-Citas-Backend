using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Repositories.Interfaces
{
    public interface IPatientRepository : IBaseRepository<Patient>
    {
        Task<Result<Patient>> CreateWithUserAsync(Patient patient, User user);
        Task<Result<Patient>> GetByUserIdAsync(int userId);
    }
}
