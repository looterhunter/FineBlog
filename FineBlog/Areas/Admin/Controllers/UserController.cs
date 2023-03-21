using AspNetCoreHero.ToastNotification.Abstractions;
using FineBlog.Models;
using FineBlog.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FineBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly INotyfService _notification;

        public UserController(UserManager<ApplicationUser> userManager, 
                              SignInManager<ApplicationUser> signInManager, 
                              INotyfService notification)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notification = notification;
        }

        public IActionResult Index()
        {
            return View();
        }

        //原本為Admin/User/Login, 改為localhost/Login
        [HttpGet("Login")]
        public IActionResult Login()
        {
            if (!HttpContext.User.Identity!.IsAuthenticated)
            {
                return View(new LoginVM());
            }

            return RedirectToAction("Index", "User", new { area = "Admin" });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginVM vm)
        {
                 
            if (!ModelState.IsValid) { return View(vm); }

            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == vm.Username);
            if(existingUser== null)
            {
                _notification.Error("找不到使用者");
                return View(vm);
            }

            var verifyPassword = await _userManager.CheckPasswordAsync(existingUser, vm.Password);
            if (!verifyPassword)
            {
                _notification.Error("密碼錯誤");
                return View(vm);
            }

            await _signInManager.PasswordSignInAsync(existingUser, vm.Password, vm.RememberMe, true);
            _notification.Success("登入成功");
            return RedirectToAction("Index", "User", new { area = "Admin" });

        }

        [HttpPost]
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            _notification.Success("成功登出");
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
