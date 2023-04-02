using AspNetCoreHero.ToastNotification.Abstractions;
using FineBlog.Data;
using FineBlog.Models;
using FineBlog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace FineBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _notification;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PageController(ApplicationDbContext context, 
                                INotyfService notification, 
                                IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _notification = notification;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> About()
        {
            var page = await _context.Pages.FirstOrDefaultAsync(x => x.Slug == "about");
            var vm = new PageVM()
            {
                Id = page!.Id,
                Title = page.Title,
                ShortDescription = page.ShortDescription,
                Description = page.Description,
                ThumbnailUrl = page.ThumbnailUrl
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> About(PageVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); }
            var page = await _context.Pages.FirstOrDefaultAsync(x => x.Slug == "about");
            if (page == null)
            {
                _notification.Error("About not found");
                return RedirectToAction("About", "Page", new { area = "Admin" });
            }

            page.Title = vm.Title;
            page.Description = vm.Description;
            page.ShortDescription = vm.ShortDescription;

            if (vm.Thumbnail != null)
            {
                _ = DeleteImage(page.ThumbnailUrl); //先刪除舊的圖片
                page.ThumbnailUrl = UploadImage(vm.Thumbnail);
            }

            await _context.SaveChangesAsync();
            _notification.Success("About Page更新成功");
            return RedirectToAction("About", "Page", new { area = "Admin" });
        }

        [HttpGet]
        public async Task<IActionResult> Contact()
        {
            var page = await _context.Pages.FirstOrDefaultAsync(x => x.Slug == "contact");
            var vm = new PageVM()
            {
                Id = page!.Id,
                Title = page.Title,
                ShortDescription = page.ShortDescription,
                Description = page.Description,
                ThumbnailUrl = page.ThumbnailUrl
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Contact(PageVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); }
            var page = await _context.Pages.FirstOrDefaultAsync(x => x.Slug == "contact");
            if (page == null)
            {
                _notification.Error("Contact Page not found");
                return RedirectToAction("Contact", "Page", new { area = "Admin" });
            }

            page.Title = vm.Title;
            page.Description = vm.Description;
            page.ShortDescription = vm.ShortDescription;

            if (vm.Thumbnail != null)
            {
                _ = DeleteImage(page.ThumbnailUrl); //先刪除舊的圖片
                page.ThumbnailUrl = UploadImage(vm.Thumbnail);
            }

            await _context.SaveChangesAsync();
            _notification.Success("Contact Page更新成功");
            return RedirectToAction("Contact", "Page", new { area = "Admin" });
        }


        [HttpGet]
        public async Task<IActionResult> Privacy()
        {
            var page = await _context.Pages.FirstOrDefaultAsync(x => x.Slug == "privacy");
            var vm = new PageVM()
            {
                Id = page!.Id,
                Title = page.Title,
                ShortDescription = page.ShortDescription,
                Description = page.Description,
                ThumbnailUrl = page.ThumbnailUrl
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Privacy(PageVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); }
            var page = await _context.Pages.FirstOrDefaultAsync(x => x.Slug == "privacy");
            if (page == null)
            {
                _notification.Error("Privacy Page not found");
                return RedirectToAction("Privacy", "Page", new { area = "Admin" });
            }

            page.Title = vm.Title;
            page.Description = vm.Description;
            page.ShortDescription = vm.ShortDescription;

            if (vm.Thumbnail != null)
            {
                _ = DeleteImage(page.ThumbnailUrl); //先刪除舊的圖片
                page.ThumbnailUrl = UploadImage(vm.Thumbnail);
            }

            await _context.SaveChangesAsync();
            _notification.Success("Privacy Page更新成功");
            return RedirectToAction("Privacy", "Page", new { area = "Admin" });
        }


        private string UploadImage(IFormFile file)
        {
            string uniqueFileName = "";
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "thumbnails");
            uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(folderPath, uniqueFileName);
            using (FileStream fileStream = System.IO.File.Create(filePath))
            {
                file.CopyTo(fileStream);//上傳的檔案內容file，copyto新的檔案內容filePath
            }

            return uniqueFileName;
        }

        private bool DeleteImage(string fileName)
        {
            if (fileName == null) { return false; }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "thumbnails", fileName);
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo != null)
            {
                System.IO.File.Delete(filePath);
                fileInfo.Delete();
            }

            return true;

        }


    }
}
