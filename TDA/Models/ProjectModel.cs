namespace TDA.Models
{
	public class Project
	{
		public int ProjectId { get; set; }
		public string ProjectName { get; set; }
		public string Description { get; set; }
	
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public int LeadId { get; set; }	

		public User Lead { get; set; }
		public ICollection<ProjectParticipant> ProjectParticipants { get; set; }
		public ICollection<ActualTask> Tasks { get; set; }
	}
	
}
