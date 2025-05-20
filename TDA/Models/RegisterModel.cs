using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TDA.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Не указан Логин")]
		public string Login { get; set; }

		[Required(ErrorMessage = "Не указан Email")]
		[EmailAddress(ErrorMessage = "Некорректный формат email")]
		public string Email { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}
