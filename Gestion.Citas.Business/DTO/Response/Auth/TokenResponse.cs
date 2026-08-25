using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.DTO.Response.Auth
{
    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
    }
}
