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
	

	public class DevelopmentController : Controller
	{
		TdaDbcontext db;
		public DevelopmentController(TdaDbcontext context)
		{
			db = context;
		}



		[Authorize(Roles = "admin, manager")]
		[HttpGet]
		public IActionResult CreateProject()
		{
			
			var users = db.Users.Select(u => new { u.UserId, u.username }).ToList();
			
			ViewBag.UsersList = users;

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateProject(ProjectModel model)
		{
			var userList = db.Users.Select(u => new { u.UserId, u.username }).ToList();


			if (ModelState.IsValid)
			{
				Project? proj = await db.Projects.FirstOrDefaultAsync(u => u.project_name == model.Name);
				if (proj == null)
				{
					proj = new Project { project_name = model.Name, Description = model.Description, CreatedAt = DateTime.Now };
					User? lead = await db.Users.FirstOrDefaultAsync(u => u.UserId == model.LeadId);
					if (lead != null)
					{
						proj.Lead = lead;
					}
					else
					{
						ModelState.AddModelError("", "Некорректные данные руководителя");
						 userList = db.Users.Select(u => new { u.UserId, u.username }).ToList();
						ViewBag.UsersList = userList;
						return View();
					}
					db.Projects.Add(proj);
					await db.SaveChangesAsync();
					return RedirectToAction("ViewProject", "Development");
				}
				else
					ModelState.AddModelError("", "Некорректные данные");
			}
			else
			{
				ModelState.AddModelError("", "Некорректные данные");
			}
			userList = db.Users.Select(u => new { u.UserId, u.username }).ToList();
			ViewBag.UsersList = userList;
			return View(model);
		}


		[Authorize(Roles = "admin, manager, user")]
		[HttpGet]
		public IActionResult ViewProject()
		{
			return View(db.Projects.Include(p => p.Lead).ToList());
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id != null)
			{
				Project? project = await db.Projects.FirstOrDefaultAsync(p => p.projectid == id);
				if (project != null)

					return View(project);
			}
			return NotFound();
		}
		[HttpPost]
		public async Task<IActionResult> Edit(Project model, int id)
		{
			Project? proj = await db.Projects.FirstOrDefaultAsync(p => p.projectid == id);
			if (proj != null)
			{
				proj.project_name = model.project_name;
				proj.Description = model.Description;
				proj.UpdatedAt = DateTime.Now;
				//   proj.LeadId = model.LeadId;
				User? lead = await db.Users.FirstOrDefaultAsync(u => u.UserId == model.LeadId);
				proj.Lead = lead;
				db.Projects.Update(proj);
				await db.SaveChangesAsync();
			}
			return RedirectToAction("ViewProject");
		}

		[HttpGet]
		[ActionName("Delete")]
		public async Task<IActionResult> ConfirmDelete(int? id)
		{
			if (id != null)
			{
				Project? project = await db.Projects.FirstOrDefaultAsync(p => p.projectid == id);
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
				Project? project = await db.Projects.FirstOrDefaultAsync(p => p.projectid == id);
				if (project != null)
				{
					db.Projects.Remove(project);
					await db.SaveChangesAsync();
					return RedirectToAction("ViewProject");
				}
			}
			return NotFound();
		}


		
		[Authorize(Roles = "admin, user")]
		[HttpGet]
		public IActionResult ViewTasks()
		{
			return View(db.Tasks.Where(q => q.Status.StatusId == 1 || q.Status.StatusId == 2 || q.Status.StatusId == 3)
				.Include(d => d.Project)
				.Include(e => e.Priority)
				.Include(w => w.Status)
				.Include(w => w.AssignedUser)
				.ToList());
		}

		public async Task<IActionResult> ViewSpecificTask(int id)
		{
			string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			int currentUserId = 0;
			if (!string.IsNullOrEmpty(userIdString))
			{
				int.TryParse(userIdString, out currentUserId);
			}
			ViewBag.CurrentUserId = currentUserId;


			ActualTask? task = await db.Tasks.Include(t => t.Project)
				.Include(t => t.Status)
				.Include(t => t.Priority)
				.Include(t => t.AssignedUser)
				.FirstOrDefaultAsync(t => t.TaskId == id);

			if (task == null)
			{
				return NotFound();
			}

			var commitInfos = GetCommitInfosForTask(id);
			ViewBag.CommitInfos = commitInfos;

			var comments = db.TaskComments
			.Where(c => c.TaskId == id)
			.Include(c => c.User)
			.OrderByDescending(c => c.CreatedAt)
			.ToList();

			ViewBag.Comments = comments;

			return View(task);
		}


		[HttpPost]
		public async Task<IActionResult> AcceptTaskAndClose(int taskId)
		{
			ActualTask? task = await db.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.TaskId == taskId);
			if (task == null)
			{
				return NotFound();
			}

			// Получить LeadId проекта
			int? leadId = task.Project?.LeadId;

			// Получить текущего пользователя из базы по UserId
			string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdString))
			{
				return Unauthorized();
			}
			if (!int.TryParse(userIdString, out int userId))
			{
				return Unauthorized();
			}

			if (leadId != userId)
			{
				return Forbid(); // только Lead может принимать
			}

			// обновляем статус задачи на "Закрыто"
			Status? statusClosed = await db.Statuses.FirstOrDefaultAsync(s => s.StatusMessage == "Завершена");
			if (statusClosed != null)
			{
				task.StatusId = statusClosed.StatusId;
				task.UpdatedAt = DateTime.Now;
				await db.SaveChangesAsync();
			}
			var commits = GetCommitInfosForTask(taskId);
			foreach (var commit in commits)
			{
				// ищем пользователя по email или username
				User? user = await db.Users.FirstOrDefaultAsync(u => u.username == commit.Username);
				if (user != null)
				{
					Notification notification = new Notification
					{
						UserId = user.UserId,
						Message = $"Ваш коммит по задаче '{task.Title}' был одобрен и задача закрыта.",
						CreatedAt = DateTime.Now,
						//IsRead = false
					};
					db.Notifications.Add(notification);
				}
				await db.SaveChangesAsync();
			}
			return RedirectToAction("ViewTasks");
		}

		// метод для отклонения задачи (статус - "На доработке")
		[HttpPost]
		public async Task<IActionResult> RejectTaskAndRework(int taskId)
		{
			ActualTask? task = await db.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.TaskId == taskId);
			if (task == null)
			{
				return NotFound();
			}

			int? leadId = task.Project?.LeadId;

			string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!int.TryParse(userIdString, out int userId))
			{
				return Unauthorized();
			}

			if (leadId != userId)
			{
				return Forbid();
			}

			// Обновляем статус задачи
			Status? statusRework = await db.Statuses.FirstOrDefaultAsync(s => s.StatusMessage == "На доработке");
			if (statusRework != null)
			{
				task.StatusId = statusRework.StatusId;
				task.UpdatedAt = DateTime.Now;
				await db.SaveChangesAsync();
			}
			var commits = GetCommitInfosForTask(taskId);
			foreach (var commit in commits)
			{
				// Ищем пользователя по email или username
				User? user = await db.Users.FirstOrDefaultAsync(u => u.username == commit.Username);
				if (user != null)
				{
					Notification notification = new Notification
					{
						UserId = user.UserId,
						Message = $"Ваш коммит по задаче '{task.Title}' не был принят, задача отправлена на доработку.",
						CreatedAt = DateTime.Now,
						//IsRead = false
					};
					db.Notifications.Add(notification);
				}
			}
			await db.SaveChangesAsync();

			return RedirectToAction("ViewTasks");
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateTask(ActualTaskModel model)
		{
			// Определяем допустимые значения приоритетности
			string[]? validPriorities = new[] { "Низкий", "Средний", "Высокий", "Критический" };

			// Проверяем, входит ли введённое значение в допустимый список
			if (!validPriorities.Contains(model.Priority))
			{
				ModelState.AddModelError("Priority", "Некорректное значение приоритетности");
			}

			// Проверяем все ошибки ModelState
			var errors = ModelState.Values.SelectMany(v => v.Errors).ToList();
			foreach (var error in errors)
			{
				Console.WriteLine(error.ErrorMessage);
			}

			if (ModelState.IsValid)
			{
				// Поиск существующей задачи по описанию
				ActualTask? actualtask = await db.Tasks.FirstOrDefaultAsync(u => u.Description == model.Description);
				if (actualtask == null)
				{
					actualtask = new ActualTask { Description = model.Description, Title = model.Title };
					actualtask.CreatedAt = DateTime.Now;
					actualtask.Description = model.Description;
					actualtask.Deadline = model.Deadline;

					// Связь с проектом
					Project? proj = await db.Projects.FirstOrDefaultAsync(r => r.project_name == model.Project);
					if (proj != null)
					{
						actualtask.Project = proj;
					}
					else
					{
						ModelState.AddModelError("", "Проект не найден");
						return View(model);
					}

						// Установка статуса
						Status? taskstatus = await db.Statuses.FirstOrDefaultAsync(r => r.StatusMessage == "Актуальный");
					if (taskstatus != null)
						actualtask.Status = taskstatus;

					// Установка приоритета
					Priority? taskpriority = await db.Priorities.FirstOrDefaultAsync(p => p.PriorityMessage == model.Priority);
					if (taskpriority != null)
						actualtask.Priority = taskpriority;

					db.Tasks.Add(actualtask);
					await db.SaveChangesAsync();

					return RedirectToAction("ViewTasks", "Development");
				}
				else
				{
					ModelState.AddModelError("", "Задача с таким описанием уже существует");
				}
			}
			return View(model);
		}

		public IActionResult TasksByProject(int projectId)
		{
			// Получение проекта и связанных задач
			Project? project = db.Projects
				.Include(p => p.Tasks)
					.ThenInclude(t => t.Priority)
				.Include(p => p.Tasks)
					.ThenInclude(t => t.Status)
				.FirstOrDefault(p => p.projectid == projectId);

			if (project == null)
			{
				return NotFound();
			}

			var tasks = project.Tasks;

			return View(tasks);
		}
		public IActionResult CreateTask(string projectName = null)
		{
			var model = new ActualTaskModel();
			if (!string.IsNullOrEmpty(projectName))
			{
				model.Project = projectName;
			}
			return View(model);
		}

		public async Task<IActionResult> EditTask(int? id)
		{
			if (id != null)
			{
				ActualTask? actualtask = await db.Tasks.FirstOrDefaultAsync(p => p.TaskId == id);

				if (actualtask != null)

					return View(actualtask);
			}
			return NotFound();
		}
		[HttpPost]
		public async Task<IActionResult> EditTask(ActualTask model, int id)
		{
			ActualTask? actualtask = await db.Tasks.FirstOrDefaultAsync(t => t.TaskId == id);
			if (actualtask != null)
			{
				actualtask.Title = model.Title;
				actualtask.Description = model.Description;
				actualtask.UpdatedAt = DateTime.Now;
				actualtask.Deadline = model.Deadline;
				Project? proj = await db.Projects.FirstOrDefaultAsync(r => r.project_name == model.Project.project_name);
				//if (proj != null)

				actualtask.Project = proj;

				Priority? taskpriority = await db.Priorities.FirstOrDefaultAsync(p => p.PriorityMessage == model.Priority.PriorityMessage);
				if (taskpriority != null)
				{
					actualtask.Priority = taskpriority;
				}

			}

			db.Tasks.Update(actualtask);
			await db.SaveChangesAsync();
			return RedirectToAction("ViewTasks");
		}

		[HttpGet]
		[ActionName("DeleteTask")]
		public async Task<IActionResult> ConfirmDeleteTask(int? id)
		{
			if (id != null)
			{
				ActualTask? actualtask = await db.Tasks.FirstOrDefaultAsync(p => p.TaskId == id);
				if (actualtask != null)
					return View(actualtask);
			}
			return NotFound();
		}
		[HttpPost]
		public async Task<IActionResult> DeleteTask(int? id)
		{
			if (id != null)
			{
				ActualTask? actualtask = await db.Tasks.FirstOrDefaultAsync(p => p.TaskId == id);
				if (actualtask != null)
				{
					db.Tasks.Remove(actualtask);
					await db.SaveChangesAsync();
					return RedirectToAction("ViewTasks");
				}
			}
			return NotFound();
		}

		[HttpPost]
		[Authorize] 
		public async Task<IActionResult> AcceptTask(int taskId)
		{
			var task = await db.Tasks.FindAsync(taskId);
			if (task == null)
			{
				return NotFound();
			}

			var inProgressStatus = await db.Statuses
				.FirstOrDefaultAsync(s => s.StatusMessage == "В процессе");

			// Получить текущего пользователя
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!int.TryParse(userIdString, out int userId))
			{
				return Unauthorized();
			}

			var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
			if (user == null)
			{
				return Unauthorized();
			}

			task.StatusId = inProgressStatus.StatusId;
			task.AssignedUserId = user.UserId;
			task.UpdatedAt = DateTime.Now;

			await db.SaveChangesAsync();

			return RedirectToAction("ViewTasks");
		}			
		private string connectionString ="server=localhost;database=taskdb;UserId=root;password=mysql;";
		private List<CommitInfo> GetCommitInfosForTask(int taskId)
		{
			var commits = new List<CommitInfo>();

			MySqlConnection connection = new MySqlConnection(connectionString);
			connection.Open();
			string query = "SELECT commitId, username, email, compareurl, message, taskid FROM commitinfos WHERE taskid = @taskId";
			MySqlCommand cmd = new MySqlCommand();
			cmd.Connection = connection;
			cmd.Parameters.AddWithValue("@taskid", taskId);
			cmd.CommandText = query;
			var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				commits.Add(new CommitInfo
				{
					CommitId = reader.GetInt32("commitId"),
					Username = reader.GetString("username"),
					Email = reader.GetString("email"),
					CompareUrl = reader.GetString("compareurl"),
					Message = reader.GetString("message"),
					TaskId = reader.GetInt32("taskid")
				});
			}
			connection.Close();
			return commits;
		}
	
		[HttpGet]
		public IActionResult GetComments(int taskId)
		{
			var comments = db.TaskComments
				.Where(c => c.TaskId == taskId)
				.Include(c => c.User)
				.OrderByDescending(c => c.CreatedAt)
				.ToList();

			return PartialView("_CommentsPartial", comments);
		}

		// Метод для добавления комментария
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddComment(int taskId, string commentText)
		{
			if (string.IsNullOrWhiteSpace(commentText))
			{
				// Можно вернуть ошибку или просто перезагрузить комментарии
				return RedirectToAction("ViewSpecificTask", new { id = taskId });
			}

			// Получение текущего пользователя
			string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!int.TryParse(userIdString, out int userId))
			{
				return Unauthorized();
			}

			User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
			if (user == null)
			{
				return Unauthorized();
			}

			TaskComment comment = new TaskComment
			{
				TaskId = taskId,
				UserId = user.UserId,
				CommentText = commentText,
				CreatedAt = DateTime.Now
			};

			db.TaskComments.Add(comment);
			await db.SaveChangesAsync();

			var currentCommentUserId = user.UserId;

			var task = await db.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.TaskId == taskId );

			if (task != null && task.Project != null)
			{
				int leadId = task.Project.LeadId;

				// Получаем список всех уникальных UserId, оставивших комментарии по этой задаче, кроме текущего пользователя
				var userIdsWhoCommented = await db.TaskComments
					.Where(c => c.TaskId == taskId)
					.Select(c => c.UserId)
					.Distinct()
					.ToListAsync();

				userIdsWhoCommented.Remove(user.UserId);

				// Создаем уведомления для всех участников, исключая текущего
				foreach (var recipientUserId in userIdsWhoCommented)
				{
					
					Notification? existingNotification = await db.Notifications.FirstOrDefaultAsync(n =>
						n.UserId == recipientUserId &&
						n.Message.Contains($"по задаче {task.Title}") &&
						n.CreatedAt > DateTime.Now.AddMinutes(-5) // например, чтобы не дублировать за короткое время
					);

					if (existingNotification == null)
					{
						Notification notification = new Notification
						{
							UserId = recipientUserId,
							Message = $"Появился новый комментарий по задаче {task.Title} от {user.username}",
							CreatedAt = DateTime.Now,
							//IsRead = false
						};
						db.Notifications.Add(notification);
					}
				}
				await db.SaveChangesAsync();
			}


			return RedirectToAction("ViewSpecificTask", new { id = taskId });
		}

		// Метод для удаления комментария (только для админов)
		[Authorize(Roles = "admin")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteComment(int commentId)
		{
			TaskComment? comment = await db.TaskComments.FindAsync(commentId);
			if (comment == null)
			{
				return NotFound();
			}

			// Проверка роли пользователя
			if (!User.IsInRole("admin"))
			{
				return Forbid();
			}

			db.TaskComments.Remove(comment);
			await db.SaveChangesAsync();

			// Возвращение к текущему заданию
			return RedirectToAction("ViewSpecificTask", new { id = comment.TaskId });
		}
	}
}