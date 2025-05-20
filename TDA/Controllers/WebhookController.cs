using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TDA.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using System.Xml.Linq;
using Mysqlx.Crud;

namespace TDA.Controllers
{

	[ApiController]
	public class WebhookController : Controller
	{
		private readonly TdaDbcontext _dbContext;
		public WebhookController(TdaDbcontext dbContext)
		{
			_dbContext = dbContext;
		}

		[Route("api/Main")]
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
				

				foreach (var commit in test.Commits)
				{
					string jsonString = requestBody;



					MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder();
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
						throw new Exception("error");
					}
					finally
					{
						var task = await _dbContext.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.TaskId == messageid);
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
							_dbContext.Notifications.Add(notification);
							await _dbContext.SaveChangesAsync();
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