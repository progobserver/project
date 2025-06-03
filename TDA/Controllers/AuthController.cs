using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Web.Helpers;
using TDA.Models;
using MySql.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TDA.Controllers
{
	public class AuthController : Controller
	{
		private readonly TdaDbcontext db;
		public AuthController(TdaDbcontext context)
		{
			db = context;
		}

		//авторизация в системе

		[HttpGet]
		public IActionResult Login()
		{
			if (User.Identity.IsAuthenticated)
			{
				string? userName = User.FindFirstValue(ClaimTypes.Name);
				if (!string.IsNullOrEmpty(userName))
				{
					AppState.CurrentUserName = userName;
				}
				return RedirectToAction("Index", "Home");
			}

			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginModel model)
		{
			if (ModelState.IsValid)
			{
				User? user = await db.Users.Include(u => u.Role)
												.FirstOrDefaultAsync(u => u.Username == model.Login);
				if (user != null)
				{
					if (user.Role.RoleName == "blocked")
					{
						ModelState.AddModelError("", "Пользовател с таким логином заблокирован");
						return View(model);
					}
					bool passwordMatches = Crypto.VerifyHashedPassword(user.Password, model.Password);

					if (passwordMatches)
					{
						await Authenticate(user);

						AppState.CurrentUserName = user.Username;

						return RedirectToAction("ViewProject", "Project");
					}

				}
				ModelState.AddModelError("", "Некорректные логин и(или) пароль");
			}
			return View(model);
		}
		private async Task Authenticate(User user)
		{
			var claims = new List<Claim>
		  {
			 new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username),
			 new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role?.RoleName),
			 new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
		  };

			ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
				ClaimsIdentity.DefaultRoleClaimType);

			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
		}
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			return RedirectToAction("Login", "Auth");
		}
		public IActionResult Index()
		{
			return View();
		}
	}
}
