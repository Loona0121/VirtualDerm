
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DermaAI.Models;
using DermaAI.Data;

public class ChatMessagesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ChatMessagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: CHATMESSAGES
    public async Task<IActionResult> Index()    
    {
        return View(await _context.ChatMessages.ToListAsync());
    }

    // GET: CHATMESSAGES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var chatmessage = await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.Id == id);
        if (chatmessage == null)
        {
            return NotFound();
        }

        return View(chatmessage);
    }

    // GET: CHATMESSAGES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: CHATMESSAGES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ChatSessionId,ChatSession,Role,Content,SentAt")] ChatMessage chatmessage)
    {
        if (ModelState.IsValid)
        {
            _context.Add(chatmessage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(chatmessage);
    }

    // GET: CHATMESSAGES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var chatmessage = await _context.ChatMessages.FindAsync(id);
        if (chatmessage == null)
        {
            return NotFound();
        }
        return View(chatmessage);
    }

    // POST: CHATMESSAGES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,ChatSessionId,ChatSession,Role,Content,SentAt")] ChatMessage chatmessage)
    {
        if (id != chatmessage.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(chatmessage);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChatMessageExists(chatmessage.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(chatmessage);
    }

    // GET: CHATMESSAGES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var chatmessage = await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.Id == id);
        if (chatmessage == null)
        {
            return NotFound();
        }

        return View(chatmessage);
    }

    // POST: CHATMESSAGES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var chatmessage = await _context.ChatMessages.FindAsync(id);
        if (chatmessage != null)
        {
            _context.ChatMessages.Remove(chatmessage);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ChatMessageExists(int? id)
    {
        return _context.ChatMessages.Any(e => e.Id == id);
    }
}
