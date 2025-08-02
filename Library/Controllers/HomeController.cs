using System.Diagnostics;
using Library.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    public class HomeController : Controller
    {

        [HttpGet("/")]
        public IActionResult Index()
        {
           return View();
        }
    }
}
