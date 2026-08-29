using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Implementations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Repositories.Interfaces
{
    public interface IAppointmentBlockRepository : IBaseRepository<AppointmentBlock>
    {
        Task<Result<AppointmentBlock>> ForceCreateAsync(AppointmentBlock appointmentBlock, List<Appointment> appointments);
        Task<Result<List<AppointmentBlock>>> GetByFiltersAsync(int doctorId, DateOnly? startDate, DateOnly? endDate, int userId, string role);
    }
}
