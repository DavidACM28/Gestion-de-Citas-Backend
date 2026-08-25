using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gestion.Citas.DataAccess.Entities
{
    public class Patient : BaseEntity
    {
        public int UserId { get; set; }
        public User? User { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set;  } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address {  get; set; } = string.Empty;
        public List<Appointment> Appointments { get; set; } = new();
    }
}
