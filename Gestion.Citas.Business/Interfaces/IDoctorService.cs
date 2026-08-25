using Gestion.Citas.Business.DTO.Request.Doctor;
using Gestion.Citas.Common.Helpers;

namespace Gestion.Citas.Business.Interfaces
{
    public interface IDoctorService
    {
        Task<Result> RegisterAsync(CreateDoctorRequest request);
    }
}
