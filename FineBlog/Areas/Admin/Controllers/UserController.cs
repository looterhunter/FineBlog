using Microsoft.AspNetCore.Mvc;

namespace FineBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //原本為Admin/User/Login, 改為localhost/Login
        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }
    }
}
