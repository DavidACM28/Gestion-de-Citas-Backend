using Gestion.Citas.Business.DTO.Request.BusinessHours;
using Gestion.Citas.Business.DTO.Response.BusinessHours;
using Gestion.Citas.Common.Helpers;

namespace Gestion.Citas.Business.Interfaces
{
    public interface IBusinessHoursService
    {
        Task<Result<CreateBusinessHoursResponse>> CreateAsync(CreateBusinessHoursRequest request);
        Task<Result<List<GetBusinessHoursResponse>>> GetByDoctorIdAsync(int doctorId);
        Task<Result<GetBusinessHoursResponse>> GetByIdAsync(int id);
        Task<Result<GetBusinessHoursResponse>> UpdateAsync(int id, UpdateBusinessHoursRequest request, int userId, string role);
        Task<Result> DeleteAsync(int id);
    }
}
