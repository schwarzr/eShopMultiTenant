using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();

            return Redirect($"http://localhost:8000/account/signout?ReturnUrl=http://localhost:7000/");
        }
    }
}