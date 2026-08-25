using Gestion.Citas.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<bool> UserExistsAsync(string username);
    }
}
