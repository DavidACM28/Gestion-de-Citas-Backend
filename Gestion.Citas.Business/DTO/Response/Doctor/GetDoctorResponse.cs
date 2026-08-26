using Gestion.Citas.Business.DTO.Response.Specialty;
using Gestion.Citas.Business.DTO.Response.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.DTO.Response.Doctor
{
    public class GetDoctorResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public GetUserResponse User { get; set; } = new();
        public GetSpecialtyResponse Specialty { get; set; } = new();
    }
}
