using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TDA.Models
{
	public class Commit
	{
		public int CommitId { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("timestamp")]
		public DateTime? Timestamp { get; set; }

		public string CompareUrl { get; set; }

		public string AuthorUsername { get; set; }

		// Метод получения ID из сообщения
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
