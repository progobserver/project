using System.Text.RegularExpressions;

namespace TDA.Models
{
	public class CommitInfo
	{
		public string Id { get; set; }
		public string UserName { get; set; }
		public string CompareUrl { get; set; }
		public int TaskId { get; set; }
		public string Message { get; set; }
		public DateTime Timestamp { get; set; }


		public string GetTaskIdFromMessage()
		{
			var regex = new Regex(".*#(\\d+).*");
			var match = regex.Match(Message);
			if (match.Success && match.Groups.Count > 1)
			{
				return match.Groups[1].Value;
			}
			else
			{
				throw new Exception("Неверный формат сообщения!");
			}
		}
	}
}
