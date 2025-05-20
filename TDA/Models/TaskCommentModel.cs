namespace TDA.Models
{
	public class TaskComment
	{
		public int CommentId { get; set; } 
		public int TaskId { get; set; } 
		public int UserId { get; set; } 
		public string CommentText { get; set; }
		public DateTime CreatedAt { get; set; }

		public ActualTask Task { get; set; }
		public User User { get; set; }
	}
}
