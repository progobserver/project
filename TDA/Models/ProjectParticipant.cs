using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TDA.Models
{
	public class ProjectParticipant
	{
		[Key]
		[Column(Order = 0)]
		public int ProjectId { get; set; } 
		[Key]
		[Column(Order = 1)]
		public int UserId { get; set; }   
//
		public Project Project { get; set; }
		public User User { get; set; }
	}
}
