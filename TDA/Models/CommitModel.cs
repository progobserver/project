using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TDA.Models
{
	public class Commit
	{
		public int CommitId { get; set; }
		public int ProjectId { get; set; }

	//	[JsonPropertyName("username")]
		public string UserName { get; set; }

		[JsonPropertyName("email")]
		public string Email { get; set; }

		[JsonPropertyName("message")]
		public string Message { get; set; }

		[JsonPropertyName("compare_url")]
		public string ChangesLink { get; set; }

		[JsonPropertyName("timestamp")]
		public DateTime Timestamp { get; set; }

		// Навигационные свойства
		public ActualTask Task { get; set; }

		public string GetTaskIdFromMessage()
		{
			var regex = new Regex(".*#(\\d+).*");
			var match = regex.Match(Message);
			if (match.Groups.Count > 1)
			{
				return match.Groups[1].Value;
			}
			else
			{
				throw new Exception("Неверный формат сообщения!");
			}
		}
	}
	/*public class Author
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("email")]
		public string Email { get; set; }

		[JsonPropertyName("username")]
		public string Username { get; set; }
	}*/

}
