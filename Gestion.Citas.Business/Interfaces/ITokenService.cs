using Gestion.Citas.Business.DTO.Response.Auth;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Repositories.Interfaces
{
    public interface ITokenService
    {
        Result<TokenResponse> Generate(User request);
    }
}
