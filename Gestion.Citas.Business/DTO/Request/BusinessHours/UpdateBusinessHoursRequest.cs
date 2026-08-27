namespace Gestion.Citas.Business.DTO.Request.BusinessHours
{
    public class UpdateBusinessHoursRequest
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int AppointmentDurationMin { get; set; }
    }
}
