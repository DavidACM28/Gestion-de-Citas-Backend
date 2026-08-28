using Gestion.Citas.Business.DTO.Response.Doctor;
using Gestion.Citas.Business.DTO.Response.Patient;
using Gestion.Citas.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.DTO.Response.Appointment
{
    public class CreateAppointmentResponse
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = default!;
        public string Specialty { get; set; } = default!;
        public int PatientId { get; set; }
        public string PatientName { get; set; } = default!;
        public string PatientDocumentType { get; set; } = default!;
        public string PatientDocumentNumber{ get; set; } = default!;
        public string PatientPhoneNumber{ get; set; } = default!;
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int DurationMin { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
