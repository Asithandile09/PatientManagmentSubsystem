using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PatientManagmentSubsystem.Data;
using PatientManagmentSubsystem.Models;

namespace PatientManagmentSubsystem.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Patient
        public async Task<IActionResult> Index()
        {
            var patients = await _context.Patients
                .Include(p => p.Doctor)
                .Include(p => p.Ward)
                .Include(p => p.Addresses)
                .ToListAsync();

            return View(patients);
        }


        // GET: Patient/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Doctor)
                .Include(p => p.Ward)
                .Include(p => p.Addresses)
                .Include(p => p.PatientMovements)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null) return NotFound();

            return View(patient);
        }

        //GET: Patient/Create
        public IActionResult Create()
        {
            ViewBag.Doctors = new SelectList(_context.Doctors, "DoctorId", "FirstName");
            ViewBag.Wards = new SelectList(_context.Wards, "WardId", "Name");
            ViewBag.Beds = _context.Beds.Where(b => !b.IsOccupied).Include(b => b.Ward).ToList();
            return View();
        }
        // GET: Patient/Create
        //public IActionResult Create()
        //{
        //    ViewBag.Doctors = new SelectList(_context.Doctors, "DoctorId", "FirstName");
        //    ViewBag.Wards = new SelectList(_context.Wards, "WardId", "Name");

        //    // Load all beds with ward information
        //    var beds = _context.Beds
        //        .Where(b => !b.IsOccupied)
        //        .Include(b => b.Ward)
        //        .ToList();

        //    ViewBag.Beds = beds ?? new List<Bed>();

        //    return View();
        //}

        // POST: Patient/Create  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Patient patient,
            Address address,
            int BedId,
            MedicalHistory medicalHistory)
        {
            // Remove validation for fields we'll set programmatically
            ModelState.Remove("medicalHistory.PatientId");
            ModelState.Remove("medicalHistory.Patient");
            ModelState.Remove("medicalHistory.CreatedOn");

            if (ModelState.IsValid)
            {
                // Set up patient basic info
                patient.IsActive = true;
                patient.AdmissionDate = DateTime.Now;
                patient.Addresses = new List<Address> { address };

                // Add patient first to generate PatientId
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                // Now set the PatientId in medical history and add it
                medicalHistory.PatientId = patient.PatientId;
                medicalHistory.CreatedOn = DateTime.Now;

                _context.MedicalHistories.Add(medicalHistory);
                await _context.SaveChangesAsync();

                // Handle bed assignment
                var bed = await _context.Beds.FindAsync(BedId);
                if (bed != null)
                {
                    bed.IsOccupied = true;
                    bed.PatientId = patient.PatientId;
                    _context.Update(bed);

                    // Get ward name for the movement record
                    var ward = await _context.Wards.FindAsync(patient.WardId);

                    // Create initial admission movement record
                    var admissionMovement = new PatientMovement
                    {
                        PatientId = patient.PatientId,
                        FromLocation = "Admission",
                        ToLocation = ward?.Name ?? "Ward " + patient.WardId.ToString(),
                        ToWardId = patient.WardId,
                        ToBedId = BedId,
                        MovedAt = patient.AdmissionDate,
                        Type = MovementType.Admission
                    };

                    _context.PatientMovements.Add(admissionMovement);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns if validation fails
            ViewBag.Doctors = new SelectList(_context.Doctors, "DoctorId", "FirstName", patient.DoctorId);
            ViewBag.Wards = new SelectList(_context.Wards, "WardId", "Name", patient.WardId);
            ViewBag.Beds = _context.Beds.Where(b => !b.IsOccupied).Include(b => b.Ward).ToList();
            return View(patient);
        }

        // GET: Patient/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Addresses)
                .FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null) return NotFound();

            ViewBag.Doctors = new SelectList(_context.Doctors, "DoctorId", "FirstName", patient.DoctorId);
            ViewBag.Wards = new SelectList(_context.Wards, "WardId", "Name", patient.WardId);
            return View(patient);
        }

        // POST: Patient/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Patient patient)
        {
            if (id != patient.PatientId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPatient = await _context.Patients
                        .FirstOrDefaultAsync(p => p.PatientId == id);

                    if (existingPatient == null)
                    {
                        return NotFound();
                    }

                    // Check if ward is being changed
                    if (existingPatient.WardId != patient.WardId)
                    {
                        // Create a transfer movement if ward is changing
                        var oldWard = await _context.Wards.FindAsync(existingPatient.WardId);
                        var newWard = await _context.Wards.FindAsync(patient.WardId);

                        var transferMovement = new PatientMovement
                        {
                            PatientId = existingPatient.PatientId,
                            FromLocation = oldWard?.Name ?? "Unknown Ward",
                            ToLocation = newWard?.Name ?? "Unknown Ward",
                            FromWardId = existingPatient.WardId,
                            ToWardId = patient.WardId,
                            MovedAt = DateTime.Now,
                            Type = MovementType.Transfer,
                            Notes = "Ward changed via edit"
                        };

                        _context.PatientMovements.Add(transferMovement);
                    }

                    // Update only the properties that are allowed to be edited
                    existingPatient.FirstName = patient.FirstName;
                    existingPatient.LastName = patient.LastName;
                    existingPatient.DoctorId = patient.DoctorId;
                    existingPatient.WardId = patient.WardId;

                    _context.Update(existingPatient);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Patients.Any(e => e.PatientId == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.Doctors = new SelectList(_context.Doctors, "DoctorId", "FirstName", patient.DoctorId);
            ViewBag.Wards = new SelectList(_context.Wards, "WardId", "Name", patient.WardId);
            return View(patient);
        }

        // GET: Patient/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Doctor)
                .Include(p => p.Ward)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: Patient/Delete/5 (Hard Delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                var bed = await _context.Beds.FirstOrDefaultAsync(b => b.PatientId == patient.PatientId);
                if (bed != null)
                {
                    bed.IsOccupied = false;
                    bed.PatientId = null;
                    _context.Update(bed);
                }

                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Patient/SoftDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.Ward)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient != null)
            {
                // Create discharge movement record BEFORE changing patient status
                var dischargeMovement = new PatientMovement
                {
                    PatientId = patient.PatientId,
                    FromLocation = patient.Ward?.Name ?? "Unknown Ward",
                    ToLocation = "Discharged",
                    FromWardId = patient.WardId,
                    MovedAt = DateTime.Now,
                    Type = MovementType.Discharge,
                    Notes = "Patient discharged"
                };

                _context.PatientMovements.Add(dischargeMovement);

                // Update patient status
                patient.IsActive = false;
                patient.DischargeDate = DateTime.Now;
                _context.Update(patient);

                // Free up bed
                var bed = await _context.Beds.FirstOrDefaultAsync(b => b.PatientId == patient.PatientId);
                if (bed != null)
                {
                    bed.IsOccupied = false;
                    bed.PatientId = null;
                    _context.Update(bed);
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Reactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                patient.IsActive = true;
                patient.DischargeDate = null;
                _context.Update(patient);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Search
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction(nameof(Index));

            var results = await _context.Patients
                .Where(p => p.IsActive &&
                            (p.FirstName + " " + p.LastName).Contains(query))
                .Include(p => p.Doctor)
                .Include(p => p.Ward)
                .ToListAsync();

            return View("Index", results);
        }

        // GET: RecycleBin
        public async Task<IActionResult> RecycleBin()
        {
            var dischargedPatients = await _context.Patients
                .Where(p => !p.IsActive)
                .Include(p => p.Ward)
                .ToListAsync();

            return View(dischargedPatients);
        }

        // GET: Ward Status for a patient
        public async Task<IActionResult> WardStatus(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.PatientMovements.OrderByDescending(m => m.MovedAt))
                    .ThenInclude(m => m.ToWard)
                .Include(p => p.PatientMovements)
                    .ThenInclude(m => m.FromWard)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null) return NotFound();

            // Load wards for dropdowns
            ViewBag.Wards = await _context.Wards.ToListAsync();

            return View(patient);
        }

        // POST: Quick ward transfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferToWard(int patientId, int toWardId, int? toBedId, string notes = "")
        {
            var patient = await _context.Patients
                .Include(p => p.Ward)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null) return NotFound();

            var toWard = await _context.Wards.FindAsync(toWardId);

            // Create movement record
            var movement = new PatientMovement
            {
                PatientId = patientId,
                FromLocation = patient.Ward?.Name ?? "Unknown",
                ToLocation = toWard?.Name ?? "Unknown Ward",
                FromWardId = patient.WardId,
                ToWardId = toWardId,
                ToBedId = toBedId,
                MovedAt = DateTime.Now,
                Type = MovementType.Transfer,
                Notes = notes
            };

            // Handle bed changes
            if (toBedId.HasValue)
            {
                // Free up current bed
                var currentBed = await _context.Beds
                    .FirstOrDefaultAsync(b => b.PatientId == patientId);
                if (currentBed != null)
                {
                    currentBed.IsOccupied = false;
                    currentBed.PatientId = null;
                    _context.Update(currentBed);
                }

                // Assign new bed
                var newBed = await _context.Beds.FindAsync(toBedId);
                if (newBed != null)
                {
                    newBed.IsOccupied = true;
                    newBed.PatientId = patientId;
                    _context.Update(newBed);
                }
            }

            // Update patient's current ward
            patient.WardId = toWardId;

            _context.PatientMovements.Add(movement);
            _context.Update(patient);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(WardStatus), new { id = patientId });
        }

        // POST: Discharge patient (final check-out)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DischargePatient(int patientId, string notes = "")
        {
            var patient = await _context.Patients
                .Include(p => p.Ward)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null) return NotFound();

            
            var movement = new PatientMovement
            {
                PatientId = patientId,
                FromLocation = patient.Ward?.Name ?? "Unknown Ward",
                ToLocation = "Discharged",
                FromWardId = patient.WardId,
                MovedAt = DateTime.Now,
                Type = MovementType.Discharge,
                Notes = notes
            };

            // Frees up bed
            var bed = await _context.Beds.FirstOrDefaultAsync(b => b.PatientId == patientId);
            if (bed != null)
            {
                bed.IsOccupied = false;
                bed.PatientId = null;
                _context.Update(bed);
            }

            patient.IsActive = false;
            patient.DischargeDate = DateTime.Now;
            

            _context.PatientMovements.Add(movement);
            _context.Update(patient);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}