using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.DataAccess.Entities
{
    public class AppointmentBlock : BaseEntity
    {
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
