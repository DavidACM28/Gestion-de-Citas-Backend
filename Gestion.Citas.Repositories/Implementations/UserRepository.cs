using Gestion.Citas.DataAccess;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Repositories.Implementations
{
    public class UserRepository(AppointmentsDbContext context) : BaseRepository<User>(context), IUserRepository
    {
        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Set<User>().AnyAsync(u => u.Username == username);
        }
    }
}
