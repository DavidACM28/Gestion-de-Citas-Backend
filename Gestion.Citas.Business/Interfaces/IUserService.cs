using Gestion.Citas.Business.DTO.Request.User;
using Gestion.Citas.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.Interfaces
{
    public interface IUserService
    {
        Task<Result> RegisterAsync(CreateUserRequest request);
    }
}
