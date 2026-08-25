using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.DataAccess.Entities
{
    public class Specialty : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Doctor> Doctors { get; set; } = new();
    }
}
