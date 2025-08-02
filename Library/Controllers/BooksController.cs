using Library.Data;
using Library.Messaging;
using Library.Models;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Library.Controllers
{
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly BookIndexService _bookIndexService;
        private readonly KafkaLibraryProducer _kafkaLibraryProducer;

        public BooksController(AppDbContext context, BookIndexService bookIndexServicei, KafkaLibraryProducer kafkaLibraryProducer)
        {
            _context = context;
            _bookIndexService = bookIndexServicei;
            _kafkaLibraryProducer = kafkaLibraryProducer;
        }

        // GET: /Books
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.Include(b => b.Author).Include(b => b.Category).ToListAsync();
            return View(books);
        }

        // GET: /Books/Create
        public async Task<IActionResult> Create()
        {
            var authors = await _context.Authors.ToListAsync();
            var categories = await _context.Categories.ToListAsync();

            ViewBag.Authors = new SelectList(authors, "Id", "Name");
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: /Books/Create
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> CreateDone(Book book)
        {
            if (!ModelState.IsValid)
            {
                // Refill dropdown lists
                ViewBag.Authors = new SelectList(await _context.Authors.ToListAsync(), "Id", "Name");
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");

                return View("Create", book);
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();


            // Sending message to kafka
            await _kafkaLibraryProducer.PublishBookCreatedAsync(book);

            return RedirectToAction(nameof(Index));
        }




        // GET: /Books
        public IActionResult Search()
        {

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SearchDone(string q)
        {
            var results = await _bookIndexService.SearchAsync(q);

            // Debug amaçlı, direkt dönen sonucu string olarak da gösterebilirsin
            var jsonResult = System.Text.Json.JsonSerializer.Serialize(results);

            return Content(jsonResult, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> Seed()
        {
            var author = new Author { Name = "Orhan Pamuk", Biography = "Türk yazar..." };
            var category = new Category { Name = "Roman" };
            _context.Authors.Add(author);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var books = new List<Book>
            {
                new Book { Title = "Masumiyet Müzesi", Description = "Bir aşk romanı.", AuthorId = author.Id, CategoryId = category.Id },
                new Book { Title = "Kırmızı Saçlı Kadın", Description = "Bir hayat hikayesi.", AuthorId = author.Id, CategoryId = category.Id },
                new Book { Title = "Benim Adım Kırmızı", Description = "Sanat ve cinayet.", AuthorId = author.Id, CategoryId = category.Id }
            };

            _context.Books.AddRange(books);
            await _context.SaveChangesAsync();

            foreach (var book in books)
            {
                await _bookIndexService.IndexBookAsync(book);
            }

            return Content("Seed data added!");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
      
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();


            // Sending message to kafka for deletion
            await _kafkaLibraryProducer.PublishBookDeletedAsync(id);

            return RedirectToAction(nameof(Index));
        }





    }
}
