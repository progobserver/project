using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using TDA.Models;


namespace TDA.Controllers
{	
	public static class AppState
	{
		public static string CurrentUserName { get; set; }
	}
	public class AccountController : Controller
    {   
		private readonly TdaDbcontext db;
        public AccountController(TdaDbcontext context)
        {
            db = context;
        } 

		// Просмотр своего профиля
		[HttpGet]
		public async Task<IActionResult> UserPage()
		{
			if (!User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Login", "Auth");
			}
			string? userName = User.FindFirstValue(ClaimTypes.Name);
			
			User? user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == userName);

			if (user == null)
			{
				return NotFound();
			}
				return View(user);
		}

		// Редактирование профиля
		public async Task<IActionResult> ProfileEdit(int? id)
		{
			if (id != null)
			{
				User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
				if (user != null)
					return View(user);
			}
			return NotFound();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ProfileEdit( User model, int id)
		{		
				User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
			if (user != null && model.Username != null && model.Email != null)
			{
					user.Username = model.Username;
					user.Email = model.Email;

					db.Users.Update(user);
					await db.SaveChangesAsync();
					return RedirectToAction("UserPage");
			}
			else 
			{ 			
				return View(model);
			}
		}


		//изменение пароля (пока еще не используется в представлении)
		public async Task<IActionResult> ChangePassword(int? id)
		{
			if (id != null)
			{
				User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
				if (user != null)
					return View(user);
			}
			return NotFound();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePassword(User model, int id)
		{
			User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
			if (user != null && model.Password != null)
			{
				string hashedPassword = Crypto.HashPassword(model.Password);
				user.Password = hashedPassword;		

				db.Users.Update(user);
				await db.SaveChangesAsync();
				return RedirectToAction("UserPage");
			}
			else
			{
				return View(model);
			}
		}
	}
}
