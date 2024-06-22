using System.Collections.Generic;

namespace PrescriptionAPI.Models
{
    public class Patient
    {
        public int IdPatient { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Birthdate { get; set; }

        public ICollection<Prescription> Prescriptions { get; set; }
    }
}