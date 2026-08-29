using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Gestion.Citas.Repositories.Interfaces
{
    public interface IAppointmentRepository : IBaseRepository<Appointment>
    {
        Task<Result<Appointment>> CreateWithSlotsAsync(Appointment appointment, List<AppointmentSlot> slots);
        Task<Result<Appointment>> GetByIdWithDetailsAsync(int id);
        Task<bool> HasSlotConflictAsync(int doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int appointmentId);
        Task<Result<Appointment>> UpdateWithSlotsAsync(Appointment appointment, List<AppointmentSlot> slots);
        Task<Result<Appointment>> DeleteAppointmentSlotsAsync(Appointment appointment);
        Task<Result<List<Appointment>>> GetByFiltersAsync(
            int doctorId,
            string doctorFirstName,
            string doctorLastName,
            int patientId,
            string patientFirstName,
            string patientLastName,
            int specialtyId,
            string specialtyName,
            DateOnly? startDate,
            DateOnly? endDate,
            string status,
            int pageNumber,
            int pageSize,
            string role,
            int userId
            );
        Task<List<Appointment>> ListInRange(DateOnly date, TimeOnly startTime, TimeOnly endtime, int doctorId);
    }
}
