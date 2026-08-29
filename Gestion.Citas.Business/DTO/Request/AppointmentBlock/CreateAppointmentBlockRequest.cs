namespace Gestion.Citas.Business.DTO.Request.AppointmentBlock
{
    public class CreateAppointmentBlockRequest
    {
        public int DoctorId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool Force { get; set; }
    }
}
