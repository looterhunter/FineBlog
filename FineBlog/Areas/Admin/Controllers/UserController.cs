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
            var users = await _userManager.Users.ToListAsync();
            var vm = users.Select(x => new UserVM()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                UserName = x.UserName,
                Email = x.Email
            }).ToList();  //ToList() 對應View    @model List<FineBlog.ViewModels.UserVM>

            //assign Role
            foreach(var user in vm)
            {
                var singleUser = await _userManager.FindByIdAsync(user.Id);
                var role = await _userManager.GetRolesAsync(singleUser);
                user.Role = role.FirstOrDefault();
            }
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if(existingUser == null)
            {
                _notification.Error("使用者不存在");
                return View();
            }

            var vm = new ResetPasswordVM()
            {
                Id = existingUser.Id,
                UserName = existingUser.UserName
            };
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var existingUser = await _userManager.FindByIdAsync(vm.Id);
            if(existingUser == null)
            {
                _notification.Error("使用者不存在");
                return View(vm);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
            var result = await _userManager.ResetPasswordAsync(existingUser, token, vm.NewPassword);
            if (result.Succeeded)
            {
                _notification.Success("密碼重設成功");
                return RedirectToAction(nameof(Index));
            }
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

            return RedirectToAction("Index", "Post", new { area = "Admin" });
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
            return RedirectToAction("Index", "Post", new { area = "Admin" });

        }

        [HttpPost]
        [Authorize]
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            _notification.Success("成功登出");
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpGet("AccessDenied")]
        [Authorize]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
