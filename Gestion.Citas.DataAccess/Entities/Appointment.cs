using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.DataAccess.Entities
{
    public class Appointment : BaseEntity
    {
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        public DateOnly Date {  get; set; }
        public TimeOnly StartTime { get; set; }
        public int DurationMin { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Reason {  get; set; } = string.Empty;
        public string? Note { get; set; }
        public required byte[] RowVersion { get; set; }
        public List<AppointmentSlot> Slots { get; set; } = new();
    }
}
