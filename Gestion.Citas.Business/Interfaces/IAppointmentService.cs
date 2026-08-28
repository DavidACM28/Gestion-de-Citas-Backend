using Azure.Core;
using Gestion.Citas.Business.DTO.Request.Appointment;
using Gestion.Citas.Business.DTO.Response.Appointment;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;

namespace Gestion.Citas.Business.Interfaces
{
    public interface IAppointmentService
    {
        Task<Result<CreateAppointmentResponse>> CreateAsync(CreateAppointmentRequest request, int userId, string userRole);
        Task<Result<UpdateAppointmentResponse>> UpdateAsync(UpdateAppointmentRequest request, int id, int userId, string userRole);
        Task<Result<GetAppointmentResponse>> ConfirmAsync(int id);
        Task<Result<GetAppointmentResponse>> StartAsync(int id);
        Task<Result<GetAppointmentResponse>> FinishAsync(int id);
        Task<Result<GetAppointmentResponse>> CancelAsync(int id, int userId, string userRole);
        Task<Result<GetAppointmentResponse>> DidNotAttendAsync(int id);
        Task<Result<List<GetAppointmentResponse>>> GetByFiltersAsync(
            string role,
            int userId,
            int doctorId = 0,
            string doctorFirstName = "",
            string doctorLastName = "",
            int patientId = 0,
            string patientFirstName = "",
            string patientLastName = "",
            int specialtyId = 0,
            string specialtyName = "",
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            string status = "",
            int pageNumber = 1,
            int pageSize = 10
            );
    }
}
