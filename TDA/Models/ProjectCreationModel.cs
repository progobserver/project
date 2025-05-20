using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TDA.Models
{
	public class ProjectModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Не указано Название проекта")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Не указано описание проекта")]

		public string Description { get; set; }

		[Required(ErrorMessage = "Не указан руководитель")]
		[Display(Name = "Руководитель")]
		public int LeadId { get; set; }

	}
}
