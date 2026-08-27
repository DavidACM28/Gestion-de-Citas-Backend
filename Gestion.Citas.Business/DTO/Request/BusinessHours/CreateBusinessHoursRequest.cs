namespace Gestion.Citas.Business.DTO.Request.BusinessHours
{
    public class CreateBusinessHoursRequest
    {
        public int DoctorId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int AppointmentDurationMin { get; set; }
    }
}
