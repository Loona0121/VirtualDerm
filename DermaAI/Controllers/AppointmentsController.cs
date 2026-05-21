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
            var appointments = _context.Appointments
                .Include(a => a.Patient)
                .AsQueryable();

            // Filter by patient name
            if (!string.IsNullOrEmpty(search))
            {
                appointments = appointments.Where(a =>
                    a.Patient.FullName.Contains(search) ||
                    a.DoctorName.Contains(search));
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                appointments = appointments.Where(a => a.Status == status);
            }

            // Filter by date
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
            {
                appointments = appointments.Where(a => a.ScheduledDate.Date == parsedDate.Date);
            }

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Date = date;

            return View(await appointments.OrderByDescending(a => a.ScheduledDate).ToListAsync());
        }

        // GET: APPOINTMENTS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // GET: APPOINTMENTS/Create
        public IActionResult Create()
        {
            ViewBag.Patients = _context.Patients.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int PatientId, string DoctorName, DateTime ScheduledDate, string Status, string? Notes)
        {
            var appointment = new Appointment
            {
                PatientId = PatientId,
                DoctorName = DoctorName,
                ScheduledDate = ScheduledDate,
                Status = Status ?? "Pending",
                Notes = Notes,
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: APPOINTMENTS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            ViewBag.Patients = await _context.Patients
                .OrderBy(p => p.FullName)
                .ToListAsync();

            return View(appointment);
        }

        // POST: APPOINTMENTS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int PatientId, string DoctorName, DateTime ScheduledDate, string Status, string? Notes, DateTime CreatedAt)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.PatientId = PatientId;
            appointment.DoctorName = DoctorName;
            appointment.ScheduledDate = ScheduledDate;
            appointment.Status = Status ?? "Pending";
            appointment.Notes = Notes;
            appointment.CreatedAt = CreatedAt;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET: APPOINTMENTS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // POST: APPOINTMENTS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
                _context.Appointments.Remove(appointment);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}