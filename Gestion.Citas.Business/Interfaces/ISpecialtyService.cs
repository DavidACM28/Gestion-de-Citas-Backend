using Gestion.Citas.Business.DTO.Request.Specialty;
using Gestion.Citas.Business.DTO.Response.Specialty;
using Gestion.Citas.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.Interfaces
{
    public interface ISpecialtyService
    {
        Task<Result> CreateAsync(CreateSpecialtyRequest request);
        Task<Result<GetSpecialtyResponse>> GetByIdAsync(int id);
        Task<Result<List<GetSpecialtyResponse>>> ListAsync(int pageNumber = 1, int pageSize = 10);
    }
}
