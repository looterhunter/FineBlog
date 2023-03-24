using AspNetCoreHero.ToastNotification.Abstractions;
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

        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.Users.ToListAsync();
            var vm = user.Select(x => new UserVM()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                UserName = x.UserName
            }).ToList();  //ToList() 對應View    @model List<FineBlog.ViewModels.UserVM>
            return View(vm);
        }

        [Authorize(Roles ="Admin")]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterVM());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var checkUserByEmail = await _userManager.FindByEmailAsync(vm.Email);
            if(checkUserByEmail != null)
            {
                _notification.Error("Email已存在");
                return View(vm);
            }

            var checkUserByUsername = await _userManager.FindByNameAsync(vm.UserName);
            if (checkUserByUsername != null)
            {
                _notification.Error("Username已存在");
                return View(vm);
            }

            var applicationUser = new ApplicationUser()
            {
                UserName = vm.UserName,
                FirstName = vm.FisrtName,
                LastName = vm.LastName,
                Email = vm.Email
            };

            var result = await _userManager.CreateAsync(applicationUser, vm.Password);
            if (result.Succeeded)
            {
                if (vm.IsAdmin)
                {
                    await _userManager.AddToRoleAsync(applicationUser, WebsiteRoles.WebsiteAdmin);
                }
                else
                {
                    await _userManager.AddToRoleAsync(applicationUser, WebsiteRoles.WebsiteAuthor);
                }
                _notification.Success("註冊成功");
                return RedirectToAction("Index", "User", new { area = "Admin" });
            }

            return View(vm);
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
