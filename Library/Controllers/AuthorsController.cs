using Library.Data;
using Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AuthorsController : Controller
{
    private readonly AppDbContext _context;

    public AuthorsController(AppDbContext context) { _context = context; }


    public async Task<IActionResult> Index()
    {
        var authors = await _context.Authors.ToListAsync();
        return View(authors);

    }
    public IActionResult Create() => View();


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Author author)
    {
        if (!ModelState.IsValid) return View(author);
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/create")]
    public async Task<IActionResult> CreateDone(Author author)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var author = await _context.Authors.FindAsync(id);
        if (author == null)
        {
            return NotFound();
        }

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

}
