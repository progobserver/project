using Microsoft.AspNetCore.Mvc;
using TDA.Models;
using System.Security.Claims;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;

namespace TDA.Controllers
{
	[ApiController]
	public class WebhookController : Controller
	{
		private readonly TdaDbcontext db;
		public WebhookController(TdaDbcontext dbContext)
		{
			db = dbContext;
		}

		[Route("api/Main")] //для проверки
		[HttpGet]
		public async Task<string> Main1()
		{
			return "OK";
		}

		[Route("api/Report")]
		[HttpPost]
		public async Task<string> Report([FromBody] HookEventPayload test)
		{

			string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

			var webhookData = JsonConvert.DeserializeObject<HookEventPayload>(requestBody);

			string url = test.compare_url;
			if (test?.Commits != null && test.Commits.Any())
			{
				//заполняем таблицу в БД информацией о коммите
				foreach (var commit in test.Commits)
				{
					string jsonString = requestBody;

					MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder();
					//параметры строки подключения
					connectionStringBuilder.Server = "localhost";
					connectionStringBuilder.UserID = "root";
					connectionStringBuilder.Password = "mysql";
					connectionStringBuilder.Database = "taskdb";
					MySqlConnection connection = new MySqlConnection(connectionStringBuilder.ToString());

					var usernameClaim = User?.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType);
					string username = AppState.CurrentUserName;                 
					//string? username = Convert.ToString(commit.Author.Username);
					string email = Convert.ToString(commit.Author.Email);
					//string url = Convert.ToString(commit.);
					string message = Convert.ToString(commit.Message);
					//	DateTime time = commitData.Timestamp;
					int messageid = Convert.ToInt32(commit.GetTaskIdFromMessage());

					string query = "CREATE TABLE IF NOT EXISTS commitinfos (commitId INT PRIMARY KEY AUTO_INCREMENT," +
					" username VARCHAR(400), email VARCHAR(400), compareurl VARCHAR (500), message VARCHAR (500), taskid INT)";
					string addquery = $"INSERT INTO commitinfos(username, email, compareurl, message, taskid) VALUES ('{username}', '{email}', '{url}','{message}',{messageid})";
					try
					{
						connection.Open();
						MySqlCommand command = new MySqlCommand();
						command.Connection = connection;
						command.CommandText = query;
						command.ExecuteNonQuery();
						command.CommandText = addquery;
						command.ExecuteNonQuery();
						connection.Close();
					}
					catch
					{
						throw new Exception("Error");
					}
					finally
					{
						var task = await db.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.TaskId == messageid);
						if (task != null && task.Project != null)
						{
							int leadId = task.Project.LeadId;

							// Создаем уведомление для Lead
							var notification = new Notification
							{
								UserId = leadId,
								Message = $"Новый коммит по задаче {task.Title}: {commit.Message}",
								CreatedAt = DateTime.Now,
								//IsRead = false
							};
							db.Notifications.Add(notification);
							await db.SaveChangesAsync();
						}
					}
				}
			}

			if (test.Commits != null && test.Commits.Any() && test.compare_url.Any())
			{
				return "OK";
			}
			else
			{
				return "not OK";
			}
		} 
	}
}