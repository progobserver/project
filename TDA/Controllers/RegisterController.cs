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
	public class RegisterController : Controller
	{
		private readonly TdaDbcontext db;
		public RegisterController(TdaDbcontext context)
		{
			db = context;
		}
		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterModel model)
		{
			if (ModelState.IsValid)
			{
				var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Username == model.Login);
				if (existingUser != null)
				{
					ModelState.AddModelError("", "Пользователь с таким логином уже существует");
					return View(model);
				}
				var userMail = model.Email;
				var creationDate = DateTime.Now;
				string hashedPassword = Crypto.HashPassword(model.Password);
				var user = new User { Username = model.Login, Email = userMail, Password = hashedPassword, CreatedAt = creationDate };

				var userRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "user");
				if (userRole != null)
				{
					user.Role = userRole;
				}

				db.Users.Add(user);
				await db.SaveChangesAsync();

				await Authenticate(user);
				return RedirectToAction("Index", "Home");
			}
			ModelState.AddModelError("", "Некорректные данные");
			return View(model);
		}


		//Регистрация админа (вызывается при пустой таблице юзеров)
		[HttpGet]
		public IActionResult AdminReg()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AdminReg(RegisterModel model)
		{
			if (ModelState.IsValid)
			{
				var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Username == model.Login);

				var userMail = model.Email;
				var creationDate = DateTime.Now;
				string hashedPassword = Crypto.HashPassword(model.Password);
				var user = new User { Username = model.Login, Email = userMail, Password = hashedPassword, CreatedAt = creationDate };

				var userRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "admin");
				if (userRole != null)
				{
					user.Role = userRole;
				}

				db.Users.Add(user);
				await db.SaveChangesAsync();

				await Authenticate(user);
				return RedirectToAction("Index", "Home");
			}
			ModelState.AddModelError("", "Некорректные данные");
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

	}
}
