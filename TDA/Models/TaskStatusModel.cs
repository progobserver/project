namespace TDA.Models
{
	public class Status
	{
		public int StatusId { get; set; }
		public string StatusMessage { get; set; }

		public ICollection<ActualTask> tasks { get; set; }
	}
}
