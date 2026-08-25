using Gestion.Citas.DataAccess;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;

namespace Gestion.Citas.Repositories.Implementations
{
    public class SpecialtyRepository(AppointmentsDbContext context) : BaseRepository<Specialty>(context), ISpecialtyRepository
    {
    }
}
