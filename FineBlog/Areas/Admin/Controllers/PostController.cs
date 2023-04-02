using AspNetCoreHero.ToastNotification.Abstractions;
using FineBlog.Data;
using FineBlog.Models;
using FineBlog.Utilities;
using FineBlog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FineBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _notification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostController(ApplicationDbContext context,
                                INotyfService notification,
                                IWebHostEnvironment webHostEnvironment,
                                UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _notification = notification;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var listOfPosts = new List<Post>();

            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var loggedInUserRole = await _userManager.GetRolesAsync(loggedInUser!);
            if (loggedInUserRole[0] == WebsiteRoles.WebsiteAdmin)
            {
                //列出全部，權限為admin
                listOfPosts = await _context.Posts.Include(x => x.ApplicationUser).ToListAsync();
            }
            else
            {
                //只列出自已的部份，條件為登入者Id
                listOfPosts = await _context.Posts.Include(x => x.ApplicationUser).Where(x=>x.ApplicationUser!.Id == loggedInUser!.Id).ToListAsync();
            }

            var listOfPostsVM = listOfPosts.Select(x => new PostVM()
            {
                Id = x.Id,
                Title = x.Title,
                CreateDate = x.CreateDate,
                ThumbnailUrl = x.ThumbnailUrl,
                AuthorName = x.ApplicationUser!.FirstName + " " + x.ApplicationUser!.LastName
            }).ToList();
            return View(listOfPostsVM);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePostVM());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePostVM vm)
        {
            if(!ModelState.IsValid) { return View(vm); }

            //get logged in user id 
            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            
            var post = new Post();

            post.Title = vm.Title;
            post.Description = vm.Description;
            post.ShortDescription = vm.ShortDescription;
            post.ApplicationUserId = loggedInUser!.Id;

            if(vm.Title != null)
            {
                string slug = vm.Title.Trim();
                slug = slug.Replace(" ", "-");
                post.Slug = slug + "-" + Guid.NewGuid();
            }

            if(vm.Thumbnail != null)
            {
                post.ThumbnailUrl = UploadImage(vm.Thumbnail);
            }


            await _context.Posts!.AddAsync(post);
            await _context.SaveChangesAsync();

            _notification.Success("新增文章成功");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == id);

            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var loggedInUserRole = await _userManager.GetRolesAsync(loggedInUser!);

            if (loggedInUserRole[0] == WebsiteRoles.WebsiteAdmin || loggedInUser?.Id == post?.ApplicationUserId)
            {
                _ = DeleteImage(post.ThumbnailUrl); //有可能為空值(null)

                _context.Posts.Remove(post!);
                await _context.SaveChangesAsync();
                _notification.Success("Post刪除成功");
                return RedirectToAction("Index", "Post", new { area = "Admin" });
            }
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == id);
            if(post == null)
            {
                _notification.Error("找不到此文章");
                return RedirectToAction("Index","Post",new {area="Admin"});
            }

            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var loggedInUserRole = await _userManager.GetRolesAsync(loggedInUser!);

            if (loggedInUserRole[0] != WebsiteRoles.WebsiteAdmin && loggedInUser?.Id != post?.ApplicationUserId)
            {
                _notification.Error("你未受權unAuthorize");
                return RedirectToAction("Index", "Post", new { area = "Admin" });
            }

            var vm = new CreatePostVM()
            {
                Id = post.Id,
                Title = post.Title,
                ShortDescription = post.ShortDescription,
                Description = post.Description,
                ThumbnailUrl = post.ThumbnailUrl,
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreatePostVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); }

            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == vm.Id);
            if(post == null)
            {
                _notification.Error("找不到此文章");
                return RedirectToAction("Index", "Post", new { area = "Admin" });
            }

            post.Title = vm.Title;
            post.ShortDescription = vm.ShortDescription;
            post.Description = vm.Description;
            if(vm.Thumbnail != null)
            {
                _ = DeleteImage(post.ThumbnailUrl); //先刪除舊的圖片
                post.ThumbnailUrl = UploadImage(vm.Thumbnail);
            }
            await _context.SaveChangesAsync();
            _notification.Success("文章Post更新成功");
            return RedirectToAction("Index", "Post", new { area = "Admin" });
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
            //另一種上傳檔案 寫檔方法，
            /*
            var fileStream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(fileStream);
            stream.Close();
            */

            return uniqueFileName;
        }

        private bool DeleteImage(string fileName)
        {
            if(fileName == null) { return false; }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "thumbnails", fileName);
            FileInfo fileInfo = new FileInfo(filePath);
            if(fileInfo != null)
            {
                System.IO.File.Delete(filePath);
                fileInfo.Delete();
            }

            return true;

        }
    }
}
