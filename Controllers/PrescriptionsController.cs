using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrescriptionAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PrescriptionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly PrescriptionContext _context;

        public PrescriptionsController(PrescriptionContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddPrescription([FromBody] PrescriptionRequest request)
        {
            if (request.Medicaments.Count > 10)
            {
                return BadRequest("A prescription cannot contain more than 10 medicaments.");
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.FirstName == request.Patient.FirstName && p.LastName == request.Patient.LastName);

            if (patient == null)
            {
                patient = new Patient
                {
                    FirstName = request.Patient.FirstName,
                    LastName = request.Patient.LastName,
                    Birthdate = request.Patient.Birthdate
                };
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }

            var doctor = await _context.Doctors.FindAsync(request.IdDoctor);
            if (doctor == null)
            {
                return BadRequest("Doctor does not exist.");
            }

            foreach (var med in request.Medicaments)
            {
                if (!await _context.Medicaments.AnyAsync(m => m.IdMedicament == med.IdMedicament))
                {
                    return BadRequest($"Medicament with ID {med.IdMedicament} does not exist.");
                }
            }

            if (request.DueDate < request.Date)
            {
                return BadRequest("DueDate cannot be earlier than Date.");
            }

            var prescription = new Prescription
            {
                Date = request.Date,
                DueDate = request.DueDate,
                IdPatient = patient.IdPatient,
                IdDoctor = request.IdDoctor
            };
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            foreach (var med in request.Medicaments)
            {
                var prescriptionMedicament = new PrescriptionMedicament
                {
                    IdPrescription = prescription.IdPrescription,
                    IdMedicament = med.IdMedicament,
                    Dose = med.Dose,
                    Details = med.Details
                };
                _context.PrescriptionMedicaments.Add(prescriptionMedicament);
            }
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{idPatient}")]
        public async Task<IActionResult> GetPatientDetails(int idPatient)
        {
            var patient = await _context.Patients
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.PrescriptionMedicaments)
                        .ThenInclude(pm => pm.Medicament)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Doctor)
                .FirstOrDefaultAsync(p => p.IdPatient == idPatient);

            if (patient == null)
            {
                return NotFound();
            }

            var result = new
            {
                patient.IdPatient,
                patient.FirstName,
                patient.LastName,
                patient.Birthdate,
                Prescriptions = patient.Prescriptions
                    .OrderBy(pr => pr.DueDate)
                    .Select(pr => new
                    {
                        pr.IdPrescription,
                        pr.Date,
                        pr.DueDate,
                        Doctor = new
                        {
                            pr.Doctor.IdDoctor,
                            pr.Doctor.FirstName,
                            pr.Doctor.LastName
                        },
                        Medicaments = pr.PrescriptionMedicaments.Select(pm => new
                        {
                            pm.Medicament.IdMedicament,
                            pm.Medicament.Name,
                            pm.Dose,
                            pm.Details
                        }).ToList()
                    }).ToList()
            };

            return Ok(result);
        }
    }
}