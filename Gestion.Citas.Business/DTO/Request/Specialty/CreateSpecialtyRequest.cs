using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.DTO.Request.Specialty
{
    public class CreateSpecialtyRequest
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
    }
}
