using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PatientManagmentSubsystem.Data;
using PatientManagmentSubsystem.Models;

namespace PatientManagmentSubsystem.Controllers
{
    public class PatientMovementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientMovementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------ LIST ---------------
        public async Task<IActionResult> Index()
        {
            var movements = _context.PatientMovements
                                    .Include(p => p.Patient);
            return View(await movements.ToListAsync());
        }

        // -------------------- DETAILS -----------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movement = await _context.PatientMovements
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(m => m.PatientMovementId == id);

            return movement == null ? NotFound() : View(movement);
        }

        // -------------------- CREATE (GET) ------------
        public IActionResult Create(int? patientId)
        {
            var patients = _context.Patients
                .Where(p => p.IsActive)
                .Select(p => new { p.PatientId, FullName = p.FirstName + " " + p.LastName })
                .ToList();
            ViewData["PatientId"] = new SelectList(patients, "PatientId", "FullName", patientId);

            var locations = new[] { "General Ward", "ICU", "Emergency", "Pediatrics", "Radiology", "Operating Theatre" };
            ViewBag.Locations = new SelectList(locations);

            
            var wards = _context.Wards.ToList();
            ViewBag.Wards = new SelectList(wards, "WardId", "Name");
            LoadDropDowns(patientId, null, null);

            return View();
        }

        // -------------------- CREATE (POST) -----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [Bind("PatientMovementId,PatientId,FromLocation,ToLocation,MovedAt,ToWardId,ToBedId,Type,Notes")] PatientMovement patientMovement)
        {
            // Remove navigation property from validation
            ModelState.Remove("Patient");
            ModelState.Remove("FromWard");
            ModelState.Remove("ToWard");
            ModelState.Remove("ToBed");

            // Validate movement type and required bed
            if (patientMovement.ToWardId.HasValue && !patientMovement.ToBedId.HasValue)
            {
                ModelState.AddModelError("ToBedId", "Please select a bed when assigning a ward.");
            }

            // Validate date is not future
            if (patientMovement.MovedAt > DateTime.Now)
            {
                ModelState.AddModelError("MovedAt", "Movement date cannot be in the future.");
            }

            // Check bed occupancy
            if (patientMovement.ToBedId.HasValue)
            {
                var bed = await _context.Beds.FindAsync(patientMovement.ToBedId.Value);
                if (bed != null && bed.IsOccupied && bed.PatientId != patientMovement.PatientId)
                {
                    ModelState.AddModelError("ToBedId", "Selected bed is already occupied.");
                }
            }

            if (ModelState.IsValid)
            {
                // Reassign bed if needed
                if (patientMovement.ToWardId.HasValue && patientMovement.ToBedId.HasValue)
                {
                    // Free current bed
                    var currentBed = await _context.Beds.FirstOrDefaultAsync(b => b.PatientId == patientMovement.PatientId);
                    if (currentBed != null)
                    {
                        currentBed.IsOccupied = false;
                        currentBed.PatientId = null;
                        _context.Update(currentBed);
                    }

                    // Assign new bed
                    var newBed = await _context.Beds.FindAsync(patientMovement.ToBedId.Value);
                    if (newBed != null)
                    {
                        newBed.IsOccupied = true;
                        newBed.PatientId = patientMovement.PatientId;
                        _context.Update(newBed);

                        // Update patient ward
                        var patient = await _context.Patients.FindAsync(patientMovement.PatientId);
                        if (patient != null)
                        {
                            patient.WardId = patientMovement.ToWardId.Value;
                            _context.Update(patient);
                        }
                    }
                }

                _context.PatientMovements.Add(patientMovement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            LoadDropDowns(patientMovement.PatientId, patientMovement.ToWardId, patientMovement.ToBedId);
            return View(patientMovement);
        }

        // -------------------- EDIT (GET) --------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var movement = await _context.PatientMovements.FindAsync(id);
            if (movement == null) return NotFound();

            LoadDropDowns(movement.PatientId, movement.ToWardId, movement.ToBedId);
            return View(movement);
        }

        // -------------------- EDIT (POST) -------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("PatientMovementId,PatientId,FromLocation,ToLocation,MovedAt,ToWardId,ToBedId,Type,Notes")]
            PatientMovement movement)
        {
            if (id != movement.PatientMovementId) return NotFound();

            // Remove navigation property from validation
            ModelState.Remove("Patient");
            ModelState.Remove("FromWard");
            ModelState.Remove("ToWard");
            ModelState.Remove("ToBed");

            // Custom validations
            if (movement.ToWardId.HasValue && !movement.ToBedId.HasValue)
            {
                ModelState.AddModelError("ToBedId", "Please select a bed for the chosen ward.");
            }

            if (!string.IsNullOrEmpty(movement.FromLocation) &&
                !string.IsNullOrEmpty(movement.ToLocation) &&
                movement.FromLocation.Equals(movement.ToLocation, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("ToLocation", "Destination must be different from the starting location.");
            }

            if (movement.MovedAt > DateTime.Now)
            {
                ModelState.AddModelError("MovedAt", "Movement date cannot be in the future.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Bed reassignment only if ward/bed specified
                    if (movement.ToWardId.HasValue && movement.ToBedId.HasValue)
                    {
                        var currentBed = await _context.Beds.FirstOrDefaultAsync(b => b.PatientId == movement.PatientId);
                        if (currentBed != null)
                        {
                            currentBed.IsOccupied = false;
                            currentBed.PatientId = null;
                        }

                        var newBed = await _context.Beds.FindAsync(movement.ToBedId);
                        if (newBed != null)
                        {
                            newBed.IsOccupied = true;
                            newBed.PatientId = movement.PatientId;
                        }

                        // Update patient's ward
                        var patient = await _context.Patients.FindAsync(movement.PatientId);
                        if (patient != null)
                        {
                            patient.WardId = movement.ToWardId.Value;
                            _context.Update(patient);
                        }
                    }

                    _context.Update(movement);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientMovementExists(movement.PatientMovementId))
                        return NotFound();
                    else
                        throw;
                }
            }

            LoadDropDowns(movement.PatientId, movement.ToWardId, movement.ToBedId);
            return View(movement);
        }

        // -------------------- DELETE ------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movement = await _context.PatientMovements
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(m => m.PatientMovementId == id);

            return movement == null ? NotFound() : View(movement);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movement = await _context.PatientMovements.FindAsync(id);
            if (movement != null)
            {
                _context.PatientMovements.Remove(movement);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetBedsByWard(int wardId)
        {
            var beds = _context.Beds
                .Where(b => b.WardId == wardId && !b.IsOccupied)
                .Select(b => new { b.BedId, b.Name })
                .ToList();

            return Json(beds);
        }
        [HttpGet]
        public IActionResult GetPatientCurrentLocation(int patientId)
        {
            var patient = _context.Patients
                .Include(p => p.Ward)
                .FirstOrDefault(p => p.PatientId == patientId);

            string location = "Unknown";

            if (patient != null)
            {
                var wardName = patient.Ward?.Name ?? "Unknown";

                // Find the current bed (if any)
                var bed = _context.Beds.FirstOrDefault(b => b.PatientId == patientId);
                if (bed != null)
                {
                    location = $"{wardName} — Bed {bed.Name}";
                }
                else
                {
                    location = wardName;
                }
            }

            return Json(new { location });
        }



        private bool PatientMovementExists(int id)
        {
            return _context.PatientMovements.Any(e => e.PatientMovementId == id);
        }

        /// <summary>
        /// Centralized dropdown loader to avoid repetition
        /// </summary>
        private void LoadDropDowns(int? selectedPatient, int? selectedWard, int? selectedBed)
        {
            var patients = _context.Patients
                .Where(p => p.IsActive)
                .Select(p => new { p.PatientId, FullName = p.FirstName + " " + p.LastName })
                .ToList();
            ViewData["PatientId"] = new SelectList(patients, "PatientId", "FullName", selectedPatient);

            var locations = new[] { "General Ward", "ICU", "Emergency", "Pediatrics", "Radiology", "Operating Theatre" };
            ViewBag.Locations = new SelectList(locations);

            // CRITICAL FIX: Load wards properly
            var wards = _context.Wards.ToList();
            ViewBag.Wards = new SelectList(wards, "WardId", "Name", selectedWard);

            // Load beds if ward is selected
            if (selectedWard.HasValue)
            {
                var beds = _context.Beds
                    .Where(b => b.WardId == selectedWard && !b.IsOccupied)
                    .ToList();
                ViewBag.Beds = new SelectList(beds, "BedId", "Name", selectedBed);
            }
            else
            {
                ViewBag.Beds = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }
    }
}