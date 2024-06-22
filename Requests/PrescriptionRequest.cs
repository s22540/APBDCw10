using System;
using System.Collections.Generic;

namespace PrescriptionAPI.Models
{
    public class PrescriptionRequest
    {
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public PatientRequest Patient { get; set; }
        public int IdDoctor { get; set; }
        public List<MedicamentRequest> Medicaments { get; set; }
    }

    public class PatientRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Birthdate { get; set; }
    }

    public class MedicamentRequest
    {
        public int IdMedicament { get; set; }
        public int Dose { get; set; }
        public string Details { get; set; }
    }
}