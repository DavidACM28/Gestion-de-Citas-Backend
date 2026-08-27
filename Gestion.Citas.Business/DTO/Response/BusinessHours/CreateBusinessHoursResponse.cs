using Gestion.Citas.Business.DTO.Response.Doctor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.DTO.Response.BusinessHours
{
    public class CreateBusinessHoursResponse
    {
        public int id { get; set; }
        public GetDoctorResponse Doctor { get; set; } = new();
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int AppointmentDurationMin { get; set; }
    }
}
