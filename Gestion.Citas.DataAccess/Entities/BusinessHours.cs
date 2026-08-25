using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.DataAccess.Entities
{
    public class BusinessHours : BaseEntity
    {
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int AppointmentDurationMin { get; set; } = 15;
        
    }
}
