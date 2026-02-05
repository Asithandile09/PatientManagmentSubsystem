using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PatientManagmentSubsystem.Data;
using PatientManagmentSubsystem.Models;

namespace PatientManagmentSubsystem.Controllers
{
    public class BedsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BedsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Beds
        public async Task<IActionResult> Index()
        {
            var beds = await _context.Beds
                .Include(b => b.Ward)
                .Include(b => b.Patient)
                .OrderBy(b => b.Ward.Name)
                .ThenBy(b => b.Name)
                .ToListAsync();

            return View(beds);
        }

        // GET: Beds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bed = await _context.Beds
                .Include(b => b.Ward)
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(b => b.BedId == id);

            if (bed == null) return NotFound();

            return View(bed);
        }

        // GET: Beds/Create
        public IActionResult Create()
        {
            ViewBag.Wards = new SelectList(_context.Wards, "WardId", "Name");
            return View();
        }

        // POST: Beds/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BedId,Name,WardId,IsOccupied")] Bed bed)
        {
            // Remove navigation properties from validation
            ModelState.Remove("Ward");
            ModelState.Remove("Patient");

            // Check for duplicate bed name in the same ward
            var existingBed = await _context.Beds
                .FirstOrDefaultAsync(b => b.WardId == bed.WardId && b.Name == bed.Name);

            if (existingBed != null)
            {
                ModelState.AddModelError("Name", "A bed with this name already exists in the selected ward.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(bed);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Bed '{bed.Name}' has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Wards = new SelectList(_context.Wards, "WardId", "Name", bed.WardId);
            return View(bed);
        }

        // GET: Beds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bed = await _context.Beds.FindAsync(id);
            if (bed == null) return NotFound();

            ViewBag.Wards = new SelectList(_context.Wards, "WardId", "Name", bed.WardId);
            return View(bed);
        }

        // POST: Beds/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BedId,Name,WardId,IsOccupied,PatientId")] Bed bed)
        {
            if (id != bed.BedId) return NotFound();

            // Remove navigation properties from validation
            ModelState.Remove("Ward");
            ModelState.Remove("Patient");

            // Check for duplicate bed name in the same ward (excluding current bed)
            var existingBed = await _context.Beds
                .FirstOrDefaultAsync(b => b.WardId == bed.WardId && b.Name == bed.Name && b.BedId != bed.BedId);

            if (existingBed != null)
            {
                ModelState.AddModelError("Name", "A bed with this name already exists in the selected ward.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bed);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Bed '{bed.Name}' has been updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BedExists(bed.BedId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Wards = new SelectList(_context.Wards, "WardId", "Name", bed.WardId);
            return View(bed);
        }

        // GET: Beds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var bed = await _context.Beds
                .Include(b => b.Ward)
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(b => b.BedId == id);

            if (bed == null) return NotFound();

            // Check if bed is occupied
            if (bed.IsOccupied)
            {
                TempData["Error"] = "Cannot delete an occupied bed. Please move the patient first.";
                return RedirectToAction(nameof(Index));
            }

            return View(bed);
        }

        // POST: Beds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed != null)
            {
                // Double-check bed is not occupied
                if (bed.IsOccupied)
                {
                    TempData["Error"] = "Cannot delete an occupied bed.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Beds.Remove(bed);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Bed '{bed.Name}' has been deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Available beds for a specific ward (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetAvailableBeds(int wardId)
        {
            var beds = await _context.Beds
                .Where(b => b.WardId == wardId && !b.IsOccupied)
                .Select(b => new { b.BedId, b.Name })
                .ToListAsync();

            return Json(beds);
        }

        // POST: Assign patient to bed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPatient(int bedId, int patientId)
        {
            var bed = await _context.Beds.FindAsync(bedId);
            var patient = await _context.Patients.FindAsync(patientId);

            if (bed == null || patient == null)
                return NotFound();

            if (bed.IsOccupied)
            {
                TempData["Error"] = "Bed is already occupied.";
                return RedirectToAction(nameof(Index));
            }

            // Free up patient's current bed if any
            var currentBed = await _context.Beds.FirstOrDefaultAsync(b => b.PatientId == patientId);
            if (currentBed != null)
            {
                currentBed.IsOccupied = false;
                currentBed.PatientId = null;
                _context.Update(currentBed);
            }

            // Assign new bed
            bed.IsOccupied = true;
            bed.PatientId = patientId;
            _context.Update(bed);

            // Update patient's ward
            patient.WardId = bed.WardId;
            _context.Update(patient);

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Patient assigned to bed '{bed.Name}' successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Release bed (remove patient assignment)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReleaseBed(int bedId)
        {
            var bed = await _context.Beds.FindAsync(bedId);
            if (bed == null) return NotFound();

            if (!bed.IsOccupied)
            {
                TempData["Info"] = "Bed is already available.";
                return RedirectToAction(nameof(Index));
            }

            bed.IsOccupied = false;
            bed.PatientId = null;
            _context.Update(bed);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Bed '{bed.Name}' has been released and is now available.";
            return RedirectToAction(nameof(Index));
        }

        private bool BedExists(int id)
        {
            return _context.Beds.Any(e => e.BedId == id);
        }
    }
}