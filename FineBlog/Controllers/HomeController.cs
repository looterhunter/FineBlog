using FineBlog.Data;
using FineBlog.Models;
using FineBlog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FineBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(int page = 1, int pageSize = 5)
        {
            var settings = _context.Settings.ToList();
            var posts = _context.Posts.Include(x => x.ApplicationUser).ToList();

            //分頁
            var pagingInfo = new PagingInfo()
            {
                TotalItems = posts.Count(),
                ItemsPerPage = pageSize,
                CurrentPage = page
            };
            //每頁要顯示的文章數量
            posts = posts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.PagingInfo = pagingInfo;

            var vm = new HomeVM()
            {
                Title = settings[0].Title,
                ShortDescription = settings[0].ShortDescription,
                ThumbnailUrl = settings[0].ThumbnailUrl,
                Posts = posts
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}