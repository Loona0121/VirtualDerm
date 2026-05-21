using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DermaAI.Models;
using DermaAI.Data;

namespace DermaAI.Controllers
{
    public class ConsultationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConsultationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CONSULTATIONS
        public async Task<IActionResult> Index(string? search, string? date)
        {
            var consultations = _context.Consultations
                .Include(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                consultations = consultations.Where(c =>
                    c.Appointment.Patient.FullName.Contains(search) ||
                    c.DoctorNotes.Contains(search));
            }

            // FILTER DATE
            if (!string.IsNullOrEmpty(date) &&
                DateTime.TryParse(date, out var parsedDate))
            {
                consultations = consultations.Where(c =>
                    c.CreatedAt.Date == parsedDate.Date);
            }

            ViewBag.Search = search;
            ViewBag.Date = date;

            return View(await consultations
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync());
        }

        // POST: CONSULTATIONS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consultation = await _context.Consultations.FindAsync(id);

            if (consultation != null)
            {
                _context.Consultations.Remove(consultation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}