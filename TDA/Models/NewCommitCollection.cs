using MySql.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using TDA.Models;
using System.Threading.Tasks; // Модель Commit

namespace TDA.Models
{
	public  class NewCommitCollection
	{
		public static async Task AddCommit(Commit commit)
		{
			// Можно использовать EF Core для вставки, если есть контекст.
			// Или оставить работу с MySQL напрямую.

			// Для примера — использование MySQL напрямую (так как в оригинале так)
			MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder
			{
				Server = "localhost",
				UserID = "root",
				Password = "mysql",
				Database = "teamdev3"
			};
			using (MySqlConnection connection = new MySqlConnection(connectionStringBuilder.ToString()))
			{
				try
				{
					connection.Open();
					string username;

					// Получение taskId из commit.Task, если есть
					int taskId = 0;
					if (commit.Task != null)
					{
						taskId = commit.Task.TaskId;
					}

					// Вставка данных
					string insertQuery = @"
                        INSERT INTO commitinfo (uname, username, email, taskid)
                        VALUES (@uname, @username, @email, @taskid)";

					using (var cmd = new MySqlCommand(insertQuery, connection))
					{
						cmd.Parameters.AddWithValue("@uname", commit.Author ?? "");
						cmd.Parameters.AddWithValue("@username", commit.Author);
						cmd.Parameters.AddWithValue("@email", commit.AuthorEmail ?? "");
						cmd.Parameters.AddWithValue("@taskid", taskId);

						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					// Логировать или пробросить исключение
					throw new Exception("Error при добавлении коммита: " + ex.Message);
				}
			}
		}

		public static IEnumerable<Commit> GetCommits()
		{
			// Реализуйте получение из базы по необходимости
			return new List<Commit>();
		}
	}
}
