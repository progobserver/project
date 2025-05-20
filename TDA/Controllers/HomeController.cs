using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TDA.Models;

namespace TDA.Controllers
{
    public class HomeController : Controller
    {
        TdaDbcontext db;
        private readonly ILogger<HomeController> _logger;
		
		public HomeController(ILogger<HomeController> logger, TdaDbcontext dbContext)
        {
            _logger = logger;
            db = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            if (db.Users.Any())
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (int.TryParse(userIdString, out int userId))
                    {
                        if (db.Notifications != null)
                        {
                            var notifications = db.Notifications
                                .Where(n => n.UserId == userId /*&& !n.IsRead*/)
                                .OrderByDescending(n => n.CreatedAt)
                                .ToList();

                            ViewBag.Notifications = notifications;
                        }
                    }
					string? userName = User.FindFirstValue(ClaimTypes.Name);

					User? user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.username == userName);
					var inProgressStatus = await db.Statuses.Where(s => s.StatusMessage == "В процессе" || s.StatusMessage == "На проверке" || s.StatusMessage == "На доработке")
					.Select(s => s.StatusId).ToListAsync();
					if (inProgressStatus != null)
					{
						// получаем список актуальных задач
						var tasksInProgress = await db.Tasks
							.Where(t => inProgressStatus.Contains(t.StatusId) && t.AssignedUser != null && t.AssignedUser.username == userName)
							.Include(t => t.Status)
							.Include(t => t.AssignedUser)
							.ToListAsync();

						//передаем в ViewBag для отображения
						ViewBag.TasksInProgress = tasksInProgress;

					}

					var CompletedStatus = await db.Statuses.FirstOrDefaultAsync(s => s.StatusMessage == "Завершена");
					if (CompletedStatus != null)
					{
						// список решенных задач
						var completedTask = await db.Tasks
							.Where(t => t.StatusId == CompletedStatus.StatusId && t.AssignedUser != null && t.AssignedUser.username == userName)
							.Include(t => t.Status)
							.Include(t => t.AssignedUser)
							.ToListAsync();

						//передаем в ViewBag 
						ViewBag.CompletedTasks = completedTask;
					}
					return View();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                return RedirectToAction("AdminReg", "Account");
            }
		}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
