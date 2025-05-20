namespace TDA.Models
{
	public class Priority
	{
		public int PriorityId { get; set; }
		public string PriorityMessage { get; set; }

		public ICollection<ActualTask> tasks { get; set; }

	}
}
