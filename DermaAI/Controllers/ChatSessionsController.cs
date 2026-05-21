using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DermaAI.Models;
using DermaAI.Data;

namespace DermaAI.Controllers
{
    public class ChatSessionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatSessionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CHATSESSIONS
        public async Task<IActionResult> Index()
        {
            return View(await _context.ChatSessions.ToListAsync());
        }

        // GET: CHATSESSIONS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chatsession = await _context.ChatSessions
                .FirstOrDefaultAsync(m => m.Id == id);

            if (chatsession == null)
            {
                return NotFound();
            }

            return View(chatsession);
        }

        // GET: CHATSESSIONS/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CHATSESSIONS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChatSession chatsession)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chatsession);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(chatsession);
        }

        // GET: CHATSESSIONS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chatsession = await _context.ChatSessions.FindAsync(id);

            if (chatsession == null)
            {
                return NotFound();
            }

            return View(chatsession);
        }

        // POST: CHATSESSIONS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, ChatSession chatsession)
        {
            if (id != chatsession.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chatsession);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChatSessionExists(chatsession.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(chatsession);
        }

        // GET: CHATSESSIONS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chatsession = await _context.ChatSessions
                .FirstOrDefaultAsync(m => m.Id == id);

            if (chatsession == null)
            {
                return NotFound();
            }

            return View(chatsession);
        }

        // POST: CHATSESSIONS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var chatsession = await _context.ChatSessions.FindAsync(id);

            if (chatsession != null)
            {
                _context.ChatSessions.Remove(chatsession);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ChatSessionExists(int? id)
        {
            return _context.ChatSessions.Any(e => e.Id == id);
        }
    }
}