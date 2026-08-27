namespace Gestion.Citas.Business.DTO.Response.BusinessHours
{
    public class GetBusinessHoursResponse
    {
        public int Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int AppointmentDurationMin { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public string SpecialtyDescription { get; set; } = string.Empty;
    }
}
