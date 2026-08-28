using Gestion.Citas.DataAccess;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Repositories.Implementations
{
    public class AppointmentBlockRepository(AppointmentsDbContext context) : BaseRepository<AppointmentBlock>(context), IAppointmentBlockRepository
    {
    }
}
