using Library.Data;
using Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class CategoriesController : Controller
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Categories/Create
    public IActionResult Create()
    {
        return View();
    }


    // GET: Categories
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories.ToListAsync();
        return View(categories);
    }


    [HttpPost]
    public async Task<IActionResult> CreateDone(Category category)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        // Deleting if there are books linked to this categor.
        var hasBooks = await _context.Books.AnyAsync(b => b.CategoryId == id);
        if (hasBooks)
        {
            TempData["Error"] = "This category cannot be deleted because it has associated books.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

}
