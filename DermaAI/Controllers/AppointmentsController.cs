using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DermaAI.Models;
using DermaAI.Data;

namespace DermaAI.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: APPOINTMENTS
        public async Task<IActionResult> Index(string? search, string? status, string? date)
        {
            if (_context.Appointments == null)
                return NotFound("Appointments data set is missing.");

            var appointments = _context.Appointments
                .Include(a => a.Patient)
                .AsNoTracking()           // ✅ faster reads, no change tracking needed
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                appointments = appointments.Where(a =>
                    a.Patient != null &&
                    a.Patient.FullName != null &&
                    a.Patient.FullName.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
                appointments = appointments.Where(a => a.Status == status);

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
                appointments = appointments.Where(a => a.ScheduledDate.Date == parsedDate.Date);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Date = date;

            return View(await appointments
                .OrderByDescending(a => a.ScheduledDate)
                .ToListAsync());
        }

        // GET: DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Appointments == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .AsNoTracking()           // ✅ read-only, no tracking needed
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // GET: CREATE
        public async Task<IActionResult> Create()    // ✅ was sync, now async
        {
            if (_context.Patients == null)
                return NotFound("Patients data set is missing.");

            ViewBag.Patients = await _context.Patients
                .AsNoTracking()
                .OrderBy(p => p.FullName)
                .ToListAsync();            // ✅ was blocking .ToList()

            return View();
        }

        // POST: CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string PatientId,
            DateTime ScheduledDate,
            string? Status,
            string? Notes)
        {
            if (_context.Appointments == null)
                return Problem("Appointments DbSet is null.");

            // ✅ Parse to int first — avoids slow .ToString() comparison on every row
            if (!int.TryParse(PatientId, out int patientIdInt))
                return BadRequest("Invalid Patient ID.");

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == patientIdInt);  // ✅ int comparison, uses index

            if (patient == null)
                return NotFound("Patient not found.");

            var appointment = new Appointment
            {
                Patient = patient,
                ScheduledDate = ScheduledDate,
                Status = Status ?? "Pending",
                Notes = Notes,
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Appointments == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            ViewBag.Patients = await _context.Patients
                .AsNoTracking()           // ✅ read-only dropdown list
                .OrderBy(p => p.FullName)
                .ToListAsync();

            return View(appointment);
        }

        // POST: EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string PatientId,
            DateTime ScheduledDate,
            string? Status,
            string? Notes)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            // ✅ Parse to int first — avoids slow .ToString() comparison on every row
            if (!int.TryParse(PatientId, out int patientIdInt))
                return BadRequest("Invalid Patient ID.");

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == patientIdInt);  // ✅ int comparison, uses index

            if (patient == null)
                return NotFound("Patient not found.");

            appointment.Patient = patient;
            appointment.ScheduledDate = ScheduledDate;
            appointment.Status = Status ?? "Pending";
            appointment.Notes = Notes;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Appointments == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .AsNoTracking()           // ✅ read-only confirm page
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // POST: DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}