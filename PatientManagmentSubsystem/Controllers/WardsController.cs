using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientManagmentSubsystem.Data;
using PatientManagmentSubsystem.Models;

namespace PatientManagmentSubsystem.Controllers
{
    public class WardsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Wards
        public async Task<IActionResult> Index()
        {
            var wards = await _context.Wards
                .Include(w => w.Beds)   // Load related beds
                .ToListAsync();

            return View(wards);
        }

        // GET: Wards/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ward = await _context.Wards
                .Include(w => w.Beds!)
                    .ThenInclude(b => b.Patient)
                        .ThenInclude(p => p!.Doctor)
                .FirstOrDefaultAsync(w => w.WardId == id);

            if (ward == null) return NotFound();

            return View(ward);
        }

        // GET: Wards/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Wards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WardId,Name,TotalBeds")] Ward ward)
        {
            if (ward == null) return BadRequest("Ward data is required.");

            if (ModelState.IsValid)
            {
                // Set available beds = total beds initially
                ward.AvailableBeds = ward.TotalBeds;
                _context.Add(ward);
                await _context.SaveChangesAsync();

                // Auto-generate beds for the ward
                for (int i = 1; i <= ward.TotalBeds; i++)
                {
                    var bed = new Bed
                    {
                        Name = $"{ward.Name}-Bed-{i}",
                        WardId = ward.WardId,
                        IsOccupied = false
                    };
                    _context.Beds.Add(bed);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(ward);
        }

        // GET: Wards/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ward = await _context.Wards
                .Include(w => w.Beds)
                .FirstOrDefaultAsync(w => w.WardId == id);

            if (ward == null) return NotFound();

            return View(ward);
        }

        // POST: Wards/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WardId,Name,TotalBeds")] Ward ward)
        {
            if (ward == null) return BadRequest("Ward data is required.");
            if (id != ward.WardId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingWard = await _context.Wards
                        .Include(w => w.Beds)
                        .FirstOrDefaultAsync(w => w.WardId == id);

                    if (existingWard == null) return NotFound();

                    // Update ward name
                    existingWard.Name = ward.Name;

                    // If TotalBeds increased, generate new beds
                    if (ward.TotalBeds > existingWard.TotalBeds)
                    {
                        int newBeds = ward.TotalBeds - existingWard.TotalBeds;
                        for (int i = 1; i <= newBeds; i++)
                        {
                            var bed = new Bed
                            {
                                Name = $"{existingWard.Name}-Bed-{existingWard.TotalBeds + i}",
                                WardId = existingWard.WardId,
                                IsOccupied = false
                            };
                            _context.Beds.Add(bed);
                        }
                    }

                    // Update total/available beds
                    existingWard.TotalBeds = ward.TotalBeds;
                    existingWard.AvailableBeds = existingWard.Beds.Count(b => !b.IsOccupied);

                    _context.Update(existingWard);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WardExists(ward.WardId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ward);
        }

        // GET: Wards/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ward = await _context.Wards.FirstOrDefaultAsync(m => m.WardId == id);
            if (ward == null) return NotFound();

            return View(ward);
        }

        // POST: Wards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ward = await _context.Wards
                .Include(w => w.Beds)
                .FirstOrDefaultAsync(w => w.WardId == id);

            if (ward != null)
            {
                // Remove beds linked to the ward first
                _context.Beds.RemoveRange(ward.Beds);
                _context.Wards.Remove(ward);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool WardExists(int id)
        {
            return _context.Wards.Any(e => e.WardId == id);
        }
    }
}
