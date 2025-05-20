using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using TDA.Models;


namespace TDA.Controllers
{
	public static class HashHolder
    {
        public static string hash { get; set; }
    }
	public static class AppState
	{
		public static string CurrentUserName { get; set; }
	}
	public class AccountController : Controller
    {   
        TdaDbcontext _context;
        public AccountController(TdaDbcontext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterModel model)
		{
			if (ModelState.IsValid)
			{
				var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.username == model.Login);
				if (existingUser != null)
				{
					ModelState.AddModelError("", "Пользователь с таким логином уже существует");
					return View(model);
				}
                var userMail = model.Email; 
                var creationDate = DateTime.Now;
				string hashedPassword = Crypto.HashPassword(model.Password);
				var user = new User { username = model.Login, email = userMail , password = hashedPassword, created_at = creationDate };

				var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.role_name == "user");
				if (userRole != null)
				{
					user.Role = userRole;
				}

				_context.Users.Add(user);
				await _context.SaveChangesAsync();

				await Authenticate(user);
				return RedirectToAction("Index", "Home");
			}
			ModelState.AddModelError("","Некорректные данные");
            return View(model);
		}

		[HttpGet]
		public IActionResult AdminReg()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AdminReg(RegisterModel model)
		{
			if (ModelState.IsValid)
			{
				var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.username == model.Login);
				
				var userMail = model.Email;
				var creationDate = DateTime.Now;
				string hashedPassword = Crypto.HashPassword(model.Password);
				var user = new User { username = model.Login, email = userMail, password = hashedPassword, created_at = creationDate };

				var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.role_name == "admin");
				if (userRole != null)
				{
					user.Role = userRole;
				}

				_context.Users.Add(user);
				await _context.SaveChangesAsync();

				await Authenticate(user);
				return RedirectToAction("Index", "Home");
			}
			ModelState.AddModelError("", "Некорректные данные");
			return View(model);
		}

		[HttpGet]
        public IActionResult Login()
        {
			if (User.Identity.IsAuthenticated)
			{
				string? userName = User.FindFirstValue(ClaimTypes.Name);
				if (!string.IsNullOrEmpty(userName))
				{
					AppState.CurrentUserName = userName;
				}
				return RedirectToAction("Index", "Home");
			}

			return View();
		}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
              User? user = await _context.Users.Include(u => u.Role)
                                              .FirstOrDefaultAsync(u => u.username == model.Login);
                if (user != null)
                {
					if  (user.Role.role_name == "blocked")
					{
						ModelState.AddModelError("", "Пользовател с таким логином заблокирован");
						return View(model);
					}
					bool passwordMatches = Crypto.VerifyHashedPassword(user.password, model.Password);

                    if (passwordMatches)
                    {
						await Authenticate(user);

						AppState.CurrentUserName = user.username;
						
						return RedirectToAction("ViewProject", "Development");
					}
					
                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }
        private async Task Authenticate(User user)
        {
		  var claims = new List<Claim>
		  {
             new Claim(ClaimsIdentity.DefaultNameClaimType, user.username),
             new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role?.role_name),
			 new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()) 
          };
           
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            return RedirectToAction("Login", "Account");
        }

		//просмотр своего профиля
		[HttpGet]
		public async Task<IActionResult> UserPage()
		{
			if (!User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Login", "Account");
			}
			string? userName = User.FindFirstValue(ClaimTypes.Name);
			
			User? user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.username == userName);

			if (user == null)
			{
				return NotFound();
			}
				return View(user);
		}

		//редактирование профиля
		public async Task<IActionResult> ProfileEdit(int? id)
		{
			if (id != null)
			{
				User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
				if (user != null)
					return View(user);
			}
			return NotFound();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ProfileEdit( User model, int id)
		{		
				User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
			if (user != null && model.username != null && model.email != null)
			{
					user.username = model.username;
					user.email = model.email;

					_context.Users.Update(user);
					await _context.SaveChangesAsync();
					return RedirectToAction("UserPage");
			}
			else 
			{ 			
				return View(model);
			}
		}

		//просмот юзеров
		[Authorize(Roles = "admin")]
		[HttpGet]
		public IActionResult ViewUser()
		{
			return View(_context.Users.Include(r => r.Role).ToList());
		}
		
		//редактирование юзеров
		public async Task<IActionResult> EditUser(int? id)
		{
			if (id != null)
			{
				User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
				if (user != null)

					return View(user);
			}
			return NotFound();
		}
		[HttpPost]
		public async Task<IActionResult> EditUser(User model, int id)
		{
			
				User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
				if (user != null)
				{
					user.username = model.username;
					Role role = await _context.Roles.FirstOrDefaultAsync(r => r.role_name == model.Role.role_name);
					if (role != null)
					{
						user.Role = role;
					}
					user.email = model.email;

					_context.Users.Update(user);
					await _context.SaveChangesAsync();
				}
				return RedirectToAction("ViewUser");
		}

		//блокировка юзера
		[Authorize(Roles="admin")]
		[HttpGet]
		[ActionName("BlockUser")]

		public async Task<IActionResult> BlockUser(int? id)
		{
			if (id != null)
			{
				User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
				if (user != null)

					return View(user);
			}
			return NotFound();
		}
		[HttpPost]
		public async Task<IActionResult> BlockUser(int id)
		{
			if (id != null)
			{
				User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
				if (user != null)
				{
					Role role = await _context.Roles.FirstOrDefaultAsync(r => r.role_name == "blocked");
					if (role != null)
					{
						user.Role = role;
					}
					_context.Users.Update(user);
					await _context.SaveChangesAsync();

				}
			}
			return RedirectToAction("ViewUser");
		}

		//разблокировка
		[Authorize(Roles = "admin")]
		[HttpGet]
		[ActionName("UnlockUser")]
		public async Task<IActionResult> UnlockUser(int? id)
		{
			if (id != null)
			{
				User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
				if (user != null)

					return View(user);
			}
			return NotFound();
		}
		[HttpPost]
		public async Task<IActionResult> UnlockUser(int id)
		{
			if (id != null)
			{
				User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
				if (user != null)
				{
					Role? role = await _context.Roles.FirstOrDefaultAsync(r => r.role_name == "user");
					if (role != null)
					{
						user.Role = role;
					}
					_context.Users.Update(user);
					await _context.SaveChangesAsync();
				}
			}
			return RedirectToAction("ViewUser");
		}


		//удаление юзеров
		[Authorize(Roles = "admin")]
		[HttpGet]
		[ActionName("DeleteUser")]
		public async Task<IActionResult> ConfirmDelete(int? id)
		{
			if (id != null)
			{
				User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
				if (user != null)
					return View(user);
			}
			return NotFound();
		}

		[HttpPost]
		public async Task<IActionResult> DeleteUser(int? id)
		{
			if (id != null)
			{
				User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
				if (user != null)
				{
					_context.Users.Remove(user);
					await _context.SaveChangesAsync();
					return RedirectToAction("ViewUser");
				}
			}
			return NotFound();
		}
	}
}
