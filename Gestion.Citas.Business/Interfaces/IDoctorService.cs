using Gestion.Citas.Business.DTO.Request.Doctor;
using Gestion.Citas.Business.DTO.Response.Doctor;
using Gestion.Citas.Common.Helpers;

namespace Gestion.Citas.Business.Interfaces
{
    public interface IDoctorService
    {
        Task<Result> RegisterAsync(CreateDoctorRequest request);
        Task<Result<GetDoctorResponse>> GetMeAsync(int userId);
    }
}
