using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

namespace TDA.Models
{
	public class HookEventPayload
	{
		[JsonProperty("ref")]
		public string Ref { get; set; }
		
		[JsonProperty("before")]
		public string Before { get; set; }

		[JsonProperty("after")]
		public string After { get; set; }

		[JsonProperty("compare_url", Required = Required.Always) ]
		public string compare_url { get; set; }

		[JsonProperty("commits")]
		public List<Commit> Commits { get; set; }
	}

	public class Commit
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("author")]
		public Author Author { get; set; }

		[JsonProperty("committer")]
		public Committer Committer { get; set; }

		[JsonProperty("timestamp")]
		public string Timestamp { get; set; }

		[JsonProperty("added")]
		public List<string> Added { get; set; }

		[JsonProperty("removed")]
		public List<string> Removed { get; set; }

		[JsonProperty("modified")]
		public List<string> Modified { get; set; }

		public int GetTaskIdFromMessage()
		{
			var regex = new System.Text.RegularExpressions.Regex(@".*#(\d+).*");
			var match = regex.Match(Message);
			if (match.Success && match.Groups.Count > 1)
			{
				return int.Parse(match.Groups[1].Value);
			}
			else
			{
				throw new Exception("Неверный формат сообщения!");
			}
		}
	}

	public class Author
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("email")]
		public string Email { get; set; }
		[JsonProperty("username")]
		public string Username { get; set; }
	}

	public class Committer
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }
	}
}