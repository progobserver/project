using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TDA.Models;

namespace TDA.Controllers
{
	public class AdminController : Controller
	{
		private readonly TdaDbcontext db;
		public AdminController(TdaDbcontext context)
		{
			db = context;
		}

		//просмот юзеров
		[Authorize(Roles = "admin")]
		[HttpGet]
		public IActionResult ViewUser()
		{
			return View(db.Users.Include(r => r.Role).ToList());
		}

		//редактирование юзеров
		public async Task<IActionResult> EditUser(int? id)
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
		public async Task<IActionResult> EditUser(User model, int id)
		{

			User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
			if (user != null)
			{
				user.Username = model.Username;
				Role role = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == model.Role.RoleName);
				if (role != null)
				{
					user.Role = role;
				}
				user.Email = model.Email;

				db.Users.Update(user);
				await db.SaveChangesAsync();
			}
			return RedirectToAction("ViewUser");
		}

		//блокировка юзера
		[Authorize(Roles = "admin")]
		[HttpGet]
		[ActionName("BlockUser")]

		public async Task<IActionResult> BlockUser(int? id)
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
		public async Task<IActionResult> BlockUser(int id)
		{

			User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);

			if (user != null)
			{
				Role role = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "blocked");
				if (role != null)
				{
					user.Role = role;
				}
				db.Users.Update(user);
				await db.SaveChangesAsync();

			}

			return RedirectToAction("ViewUser");
		}

		//разблокировка
		[Authorize(Roles = "admin")]
		[HttpGet]
		[ActionName("UnlockUser")]
		public async Task<IActionResult> UnlockUser(int? id)
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
		public async Task<IActionResult> UnlockUser(int id)
		{
			User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
			if (user != null)
			{
				Role? role = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "user");
				if (role != null)
				{
					user.Role = role;
				}
				db.Users.Update(user);
				await db.SaveChangesAsync();
			}
			return RedirectToAction("ViewUser");
		}


		//удаление юзеров
		[Authorize(Roles = "admin")]
		[HttpGet]
		[ActionName("DeleteUser")]
		public async Task<IActionResult> ConfirmDelete(int? id)
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
		public async Task<IActionResult> DeleteUser(int? id)
		{
			if (id != null)
			{
				User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
				if (user != null)
				{
					db.Users.Remove(user);
					await db.SaveChangesAsync();
					return RedirectToAction("ViewUser");
				}
			}
			return NotFound();
		}
	}
}
