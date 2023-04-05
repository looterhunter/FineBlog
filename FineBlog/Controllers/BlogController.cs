using AspNetCoreHero.ToastNotification.Abstractions;
using FineBlog.Data;
using FineBlog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FineBlog.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _notification;

        public BlogController(ApplicationDbContext context, INotyfService notification)
        {
            _context = context;
            _notification = notification;
        }

        [HttpGet]
        public IActionResult Post(string slug)
        {
            if(slug == "")
            {
                _notification.Error("找不到文章");
                return View();
            }
            var post = _context.Posts.Include(x => x.ApplicationUser).FirstOrDefault(x => x.Slug == slug);
            if(post == null)
            {
                _notification.Error("找不到文章");
                return View();
            }

            var vm = new BlogPostVM()
            {
                Id = post.Id,
                Title = post.Title,
                AuthorName = post.ApplicationUser!.FirstName + "" + post.ApplicationUser!.LastName,
                CreateDate = post.CreateDate,
                ThumbnailUrl = post.ThumbnailUrl,
                ShortDescription = post.ShortDescription,
                Description = post.Description
            };

            return View(vm);
        }
    }
}
