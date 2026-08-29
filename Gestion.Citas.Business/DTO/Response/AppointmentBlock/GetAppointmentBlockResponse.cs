namespace Gestion.Citas.Business.DTO.Response.AppointmentBlock
{
    public class GetAppointmentBlockResponse
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
