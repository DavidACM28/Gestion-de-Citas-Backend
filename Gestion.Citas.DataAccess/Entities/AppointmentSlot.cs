using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.DataAccess.Entities
{
    public class AppointmentSlot : BaseEntity
    {
        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public DateOnly Date {  get; set; }
        public TimeOnly Time { get; set; }
    }
}
