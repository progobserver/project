using System.Collections;

namespace TDA.Models
{
	public class Role
	{
		public int RoleId { get; set; }
		public string role_name { get; set; }
		public ICollection<User> Users { get; set; }

	}
}
