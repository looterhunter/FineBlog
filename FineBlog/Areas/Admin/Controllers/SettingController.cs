using AspNetCoreHero.ToastNotification.Abstractions;
using FineBlog.Data;
using FineBlog.Models;
using FineBlog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using System.Dynamic;

namespace FineBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class SettingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _notification;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SettingController(ApplicationDbContext context, 
                                INotyfService notification, 
                                IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _notification = notification;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = _context.Settings.ToList();
            if(settings.Count > 0)
            {
                var vm = new SettingVM()
                {
                    Id = settings[0].Id,
                    SiteName = settings[0].SiteName,
                    Title = settings[0].Title,
                    ShortDescription = settings[0].ShortDescription,
                    ThumbnailUrl = settings[0].ThumbnailUrl,
                    FacebooklUrl = settings[0].FacebooklUrl,
                    TwitterUrl = settings[0].TwitterUrl,
                    GithubUrl = settings[0].GithubUrl
                };
                return View(vm);
            }

            var setting = new Setting()
            {
                SiteName = "Demo Name"
            };

            await _context.Settings.AddAsync(setting);
            await _context.SaveChangesAsync();


            var createdSettings = _context.Settings.ToList();
            var createdVm = new SettingVM()
            {
                Id = createdSettings[0].Id,
                SiteName = createdSettings[0].SiteName,
                Title = createdSettings[0].Title,
                ShortDescription = createdSettings[0].ShortDescription,
                ThumbnailUrl = createdSettings[0].ThumbnailUrl,
                FacebooklUrl = createdSettings[0].FacebooklUrl,
                TwitterUrl = createdSettings[0].TwitterUrl,
                GithubUrl = createdSettings[0].GithubUrl
            };

            return View(createdVm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SettingVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); }
            var setting = await _context.Settings.FirstOrDefaultAsync(x => x.Id == vm.Id);
            if(setting == null)
            {
                _notification.Error("有些錯誤出現");
                return View(vm);
            }
            setting.SiteName = vm.SiteName;
            setting.Title = vm.Title;
            setting.ShortDescription = vm.ShortDescription;
            setting.FacebooklUrl = vm.FacebooklUrl;
            setting.TwitterUrl = vm.TwitterUrl;
            setting.GithubUrl = vm.GithubUrl;
            if (vm.Thumbnail != null)
            {
                _ = DeleteImage(setting.ThumbnailUrl); //先刪除舊的圖片
                setting.ThumbnailUrl = UploadImage(vm.Thumbnail);
            }

            await _context.SaveChangesAsync();
            _notification.Success("設定更新成功");
            return RedirectToAction("Index", "Setting", new { area = "Admin" });
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
