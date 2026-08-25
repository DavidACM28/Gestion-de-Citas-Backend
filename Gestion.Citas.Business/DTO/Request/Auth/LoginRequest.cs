using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.DTO.Request.Auth
{
    public class LoginRequest
    {
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
