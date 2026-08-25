using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.DTO.Request.Doctor
{
    public class CreateDoctorRequest
    {
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public int SpecialtyId { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
    }
}
