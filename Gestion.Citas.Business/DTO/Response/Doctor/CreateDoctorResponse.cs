using Gestion.Citas.Business.DTO.Response.Specialty;
using Gestion.Citas.Business.DTO.Response.User;

namespace Gestion.Citas.Business.DTO.Response.Doctor
{
    public class CreateDoctorResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public GetUserResponse User { get; set; } = new();
        public GetSpecialtyResponse Specialty { get; set; } = new();
    }
}
