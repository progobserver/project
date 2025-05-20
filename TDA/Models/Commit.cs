using System;

namespace TDA.Models
{
	public class CommitInfo
	{
		public int CommitId { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string CompareUrl { get; set; }
		public string Message { get; set; }
		public int TaskId { get; set; }
	}
}