using Gestion.Citas.Business.DTO.Request.AppointmentBlock;
using Gestion.Citas.Business.DTO.Response.AppointmentBlock;
using Gestion.Citas.Common.Helpers;

namespace Gestion.Citas.Business.Interfaces
{
    public interface IAppointmentBlockService
    {
        Task<Result<CreateAppointmentBlockResponse>> CreateAsync(CreateAppointmentBlockRequest request, int userId, string role);
        Task<Result<List<GetAppointmentBlockResponse>>> GetByFiltersAsync(int doctorId, DateOnly? startDate, DateOnly? endDate, int userId, string role);
        Task<Result> DeleteAsync(int id, int userId, string role);
    }
}
