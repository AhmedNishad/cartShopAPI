using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Shopping.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Register()
        {
            return View(@"C:\Users\Lenovo\source\repos\Shopping\Shopping\Areas\Identity\Pages\Account\Register.cshtml");
        }
        public IActionResult Login()
        {
            return View(@"~\Areas\Identity\Pages\Account\Login.cshtml");
        }
    }
}