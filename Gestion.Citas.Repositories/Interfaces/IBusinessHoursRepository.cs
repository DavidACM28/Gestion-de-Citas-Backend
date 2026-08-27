using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Repositories.Interfaces
{
    public interface IBusinessHoursRepository : IBaseRepository<BusinessHours>
    {
        Task<Result<List<BusinessHours>>> GetByDoctorIdAsync(int doctorId);
        Task<Result<BusinessHours>> GetByIdWithDoctorAsync(int id);
    }
}
