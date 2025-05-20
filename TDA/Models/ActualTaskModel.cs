using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TDA.Models
{
    public class ActualTaskModel
    {
		public int TaskId { get; set; } 
		public int ProjectId { get; set; } 
		
        [Required(ErrorMessage = "Не указано название")]
		public string Title { get; set; }

		[Required(ErrorMessage ="Не указана задача")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Не указан проект")]
        public string Project { get; set; }

        [Required(ErrorMessage = "Не указана приоритетность")]
        public string Priority { get; set; }
		public int Status { get; set; }
		public DateTime? Deadline { get; set; }
		public int? AssignedUserId { get; set; } 
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }


		public string? PreselectedProject { get; set; }

	}
}
