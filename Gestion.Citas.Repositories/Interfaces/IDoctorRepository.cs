using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Repositories.Interfaces
{
    public interface IDoctorRepository : IBaseRepository<Doctor>
    {
        Task<Result<Doctor>> CreateWithUserAsync(Doctor doctor, User user);
        Task<Result<Doctor>> GetByUserIdAsync(int userId);
    }
}
