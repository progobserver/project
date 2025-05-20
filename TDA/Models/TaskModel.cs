using System.Net.Mail;

namespace TDA.Models
{
	public class ActualTask
	{
		public int TaskId { get; set; } 
		public int ProjectId { get; set; } 
		public string Title { get; set; }
		public string Description { get; set; }
		public int StatusId { get; set; } 
		public int PriorityId { get; set; }
		public DateTime? Deadline { get; set; }
		public int? AssignedUserId { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		
		

		public Project Project { get; set; }
		public User AssignedUser { get; set; }
		public Status Status { get; set; }
		public Priority Priority { get; set; }
		public ICollection<TaskComment> TaskComments { get; set; }
	}
}
