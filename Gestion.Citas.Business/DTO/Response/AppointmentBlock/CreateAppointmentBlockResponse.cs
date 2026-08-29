using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.DTO.Response.AppointmentBlock
{
    public class CreateAppointmentBlockResponse
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
