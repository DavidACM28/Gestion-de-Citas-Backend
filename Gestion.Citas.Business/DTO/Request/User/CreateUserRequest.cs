using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.DTO.Request.User
{
    public class CreateUserRequest
    {
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Role { get; set; } = default!;

    }
}
