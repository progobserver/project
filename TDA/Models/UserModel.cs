using Microsoft.AspNetCore.Identity;
using TDA.Models;
namespace TDA.Models
{
	public class User 
	{
		public int UserId { get; set; }
		public string username { get; set; }
		public string email { get; set; }
		public string password { get; set; }
		public DateTime created_at { get; set; }


		public Role Role { get; set; }
		public ICollection<ProjectParticipant> project_participants { get; set; }
		public ICollection<TaskComment> TaskComments { get; set; }

		public ICollection<Notification> Notifications { get; set; }
	}
}
