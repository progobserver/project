using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TDA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using MySql.Data.MySqlClient;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using System.Configuration;

namespace TDA.Controllers
{
	public class ProjectController : Controller
	{
		private TdaDbcontext db;
		public ProjectController(TdaDbcontext context)
		{
			db = context;
		}
	
		// Создание проекта
		// Доступ у пользователей с ролью admin и manager
		[Authorize(Roles = "admin, manager")]
		[HttpGet]
		public IActionResult CreateProject()
		{
			var users = db.Users.Select(u => new { u.UserId, u.Username }).ToList();
			ViewBag.UsersList = users;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateProject(ProjectModel model)
		{
			var userList = db.Users.Select(u => new { u.UserId, u.Username }).ToList();

			if (ModelState.IsValid)
			{
				Project? proj = await db.Projects.FirstOrDefaultAsync(u => u.ProjectName == model.Name);
				if (proj == null)
				{
					proj = new Project { ProjectName = model.Name, Description = model.Description, CreatedAt = DateTime.Now };
					User? lead = await db.Users.FirstOrDefaultAsync(u => u.UserId == model.LeadId);
					if (lead != null)
					{
						proj.Lead = lead;
					}
					else
					{
						ModelState.AddModelError("", "Некорректные данные руководителя");
						userList = db.Users.Select(u => new { u.UserId, u.Username }).ToList();
						ViewBag.UsersList = userList;
						return View();
					}
					db.Projects.Add(proj);
					await db.SaveChangesAsync();
					return RedirectToAction("ViewProject", "Project");
				}
				else
					ModelState.AddModelError("", "Некорректные данные");
			}
			else
			{
				ModelState.AddModelError("", "Некорректные данные");
			}
			userList = db.Users.Select(u => new { u.UserId, u.Username }).ToList();
			ViewBag.UsersList = userList;
			return View(model);
		}

		// Просмотр проектов
		// Доступ у всех авторизованных пользователей
		[Authorize(Roles = "admin, manager, user")]
		[HttpGet]
		public IActionResult ViewProject()
		{
			return View(db.Projects.Include(p => p.Lead).ToList());
		}

		//Редактирование проекта
		public async Task<IActionResult> Edit(int? id)
		{
			if (id != null)
			{
				
				Project? project = await db.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
				if (project == null)
				{
					return View("Error");
				}
				var users = db.Users.Select(u => new { u.UserId, u.Username }).ToList();

				ViewBag.UsersList = users;

				return View(project);
			}
			else
			{
				return View("Error");
			}
		}
		[HttpPost]
		public async Task<IActionResult> Edit(Project model, int id)
		{
			Project? proj = await db.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
			if (proj != null)
			{
				proj.ProjectName = model.ProjectName;
				proj.Description = model.Description;
				proj.UpdatedAt = DateTime.Now;
				User? lead = await db.Users.FirstOrDefaultAsync(u => u.UserId == model.LeadId);
				proj.Lead = lead;
				db.Projects.Update(proj);
				await db.SaveChangesAsync();
			}
			return RedirectToAction("ViewProject");
		}

		//Удаления проекта
		[HttpGet]
		[ActionName("Delete")]
		public async Task<IActionResult> ConfirmDelete(int? id)
		{
			if (id != null)
			{
				Project? project = await db.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
				if (project != null)
					return View(project);
			}
			return NotFound();
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id != null)
			{
				Project? project = await db.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
				if (project != null)
				{
					db.Projects.Remove(project);
					await db.SaveChangesAsync();
					return RedirectToAction("ViewProject");
				}
			}
			return NotFound();
		}
	}
}
