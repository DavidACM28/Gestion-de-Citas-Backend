namespace Gestion.Citas.Business.DTO.Request.Appointment
{
    public class UpdateAppointmentRequest
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public string Reason { get; set; } = default!;
        public string Note { get; set; } = default!;
    }
}
