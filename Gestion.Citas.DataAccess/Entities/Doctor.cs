using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.DataAccess.Entities
{
    public class Doctor : BaseEntity
    {
        public int UserId { get; set; }
        public User? User { get; set; }
        public int SpecialtyId { get; set; }
        public Specialty? Specialty { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<Appointment> Appointments { get; set; } = new();
        public List<AppointmentBlock> blocks { get; set; } = new();
        public List<BusinessHours> WorkSchedule { get; set; } = new();
    }
}
