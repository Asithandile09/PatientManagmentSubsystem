using Microsoft.AspNetCore.Mvc;
using PatientManagmentSubsystem.Data;
using PatientManagmentSubsystem.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace PatientManagmentSubsystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var patientCount = _context.Patients.Count(p => p.IsActive);
            var bedCount = _context.Beds.Count();
            var occupiedBeds = _context.Beds.Count(b => b.IsOccupied);
            var availableBeds = bedCount - occupiedBeds;

            ViewData["PatientCount"] = patientCount;
            ViewData["BedCount"] = bedCount;
            ViewData["OccupiedBeds"] = occupiedBeds;
            ViewData["AvailableBeds"] = availableBeds;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Dashboard with enhanced statistics
        public async Task<IActionResult> Dashboard()
        {
            // Total active patients
            var totalPatients = await _context.Patients
                .CountAsync(p => p.IsActive);

            // Available beds
            var availableBeds = await _context.Beds
                .CountAsync(b => !b.IsOccupied);

            // Total wards
            var totalWards = await _context.Wards.CountAsync();

            // Today's admissions
            var todayAdmissions = await _context.Patients
                .CountAsync(p => p.AdmissionDate.Date == DateTime.Today);

            // Pass to view
            ViewBag.TotalPatients = totalPatients;
            ViewBag.AvailableBeds = availableBeds;
            ViewBag.TotalWards = totalWards;
            ViewBag.TodayAdmissions = todayAdmissions;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}