using Microsoft.AspNetCore.Identity;
using TDA.Models;
namespace TDA.Models
{
	public class User 
	{
		public int UserId { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public DateTime CreatedAt { get; set; }


		public Role Role { get; set; }
		public ICollection<ProjectParticipant> ProjectParticipants { get; set; }
		public ICollection<TaskComment> TaskComments { get; set; }

		public ICollection<Notification> Notifications { get; set; } 
	}
}
