using Gestion.Citas.Business.DTO.Request.Auth;
using Gestion.Citas.Business.DTO.Response.Auth;
using Gestion.Citas.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    }
}
