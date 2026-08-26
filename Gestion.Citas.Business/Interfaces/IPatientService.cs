using Gestion.Citas.Business.DTO.Request.Patient;
using Gestion.Citas.Business.DTO.Response.Patient;
using Gestion.Citas.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.Interfaces
{
    public interface IPatientService
    {
        Task<Result> RegisterAsync(CreatePatientRequest request);
        Task<Result<GetPatientResponse>> GetMeAsync(int userId);
    }
}
