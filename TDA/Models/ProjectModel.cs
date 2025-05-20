namespace TDA.Models
{
	public class Project
	{
		public int projectid { get; set; }
		public string project_name { get; set; }
		public string Description { get; set; }
	
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public int LeadId { get; set; }	

		public User Lead { get; set; }
		public ICollection<ProjectParticipant> ProjectParticipants { get; set; }
		public ICollection<ActualTask> Tasks { get; set; }
	}
	
}
