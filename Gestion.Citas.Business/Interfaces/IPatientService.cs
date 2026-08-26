using Gestion.Citas.Business.DTO.Request.Patient;
using Gestion.Citas.Business.DTO.Response.Patient;
using Gestion.Citas.Common.Helpers;
using Gestion.Citas.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.Interfaces
{
    public interface IPatientService
    {
        Task<Result> RegisterAsync(CreatePatientRequest request);
        Task<Result<GetPatientResponse>> GetMeAsync(int userId);
        Task<Result<List<GetPatientResponse>>> ListAsync(int pageNumber = 1, int pageSize = 10);
        Task<Result<GetPatientResponse>> GetByIdAsync(int id);
    }
}
