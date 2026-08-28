using Gestion.Citas.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.DTO.Request.Appointment
{
    public class CreateAppointmentRequest
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public string Reason { get; set; } = default!;
        public string? Note { get; set; }
    }
}
