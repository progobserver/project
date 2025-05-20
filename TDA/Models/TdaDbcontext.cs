using Microsoft.EntityFrameworkCore;

namespace TDA.Models
{
	public class TdaDbcontext : DbContext
	{
		public DbSet<User> Users { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<Project> Projects { get; set; }
		public DbSet<ProjectParticipant> Participants { get; set; }
		public DbSet<ActualTask> Tasks { get; set; }
		public DbSet<TaskComment> TaskComments { get; set; }
		public DbSet<Notification> Notifications { get; set; }
		public DbSet<Status> Statuses { get; set; }
		public DbSet<Priority> Priorities { get; set; }	



		public TdaDbcontext(DbContextOptions<TdaDbcontext> options) : base(options)
		{
			Database.EnsureCreated();
		}
	/*	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseMySQL("Host=localhost;Database=taskdb;UserId=root;Password=mysql");
		}
	*/
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			//инициализация данных для Roles
			modelBuilder.Entity<Role>().HasData(new Role { RoleId = 1, role_name = "admin" },
				new Role {RoleId=2, role_name="manager"},
				new Role {RoleId=3, role_name="user"},
				new Role {RoleId=4, role_name="blocked"});

			// инициализация данных для Statuses
			modelBuilder.Entity<Status>().HasData( new Status { StatusId = 1, StatusMessage = "Актуальный" },
			  new Status { StatusId = 2, StatusMessage = "В процессе" },
			  new Status { StatusId = 3, StatusMessage = "На проверке" },
			  new Status { StatusId = 4, StatusMessage = "Завершена" },
			  new Status { StatusId = 5, StatusMessage = "На доработке" });

			// инициализация данных для Priorities
			modelBuilder.Entity<Priority>().HasData(
				new Priority { PriorityId = 1, PriorityMessage = "Низкий" },
				new Priority { PriorityId = 2, PriorityMessage = "Средний" },
				new Priority { PriorityId = 3, PriorityMessage = "Высокий" },
				new Priority { PriorityId = 4, PriorityMessage = "Критический" }
			);

			modelBuilder.Entity<Project>()
				.HasOne(p => p.Lead)
				.WithMany() 
				.HasForeignKey(p => p.LeadId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ProjectParticipant>()
				.HasKey(pp => new { pp.ProjectId, pp.UserId }); 

			modelBuilder.Entity<ProjectParticipant>()
				.HasOne(pp => pp.Project)
				.WithMany(p => p.ProjectParticipants)
				.HasForeignKey(pp => pp.ProjectId);

			modelBuilder.Entity<ProjectParticipant>()
				.HasOne(pp => pp.User)
				.WithMany(u => u.project_participants)
				.HasForeignKey(pp => pp.UserId);

			modelBuilder.Entity<ActualTask>().HasKey(p => p.TaskId);

			modelBuilder.Entity<ActualTask>()
				.HasOne(t => t.Project)
				.WithMany(p => p.Tasks)
				.HasForeignKey(t => t.ProjectId);

			modelBuilder.Entity<ActualTask>()
				.HasOne(t => t.AssignedUser)
				.WithMany()
				.HasForeignKey(t => t.AssignedUserId)
				.OnDelete(DeleteBehavior.SetNull);

			modelBuilder.Entity<TaskComment>()
				.HasOne(tc => tc.Task)
				.WithMany(t => t.TaskComments)
				.HasForeignKey(tc => tc.TaskId);

			modelBuilder.Entity<TaskComment>().HasKey(p => p.CommentId);

			modelBuilder.Entity<TaskComment>()
				.HasOne(tc => tc.User)
				.WithMany(u => u.TaskComments)
				.HasForeignKey(tc => tc.UserId);
			
			modelBuilder.Entity<Status>(entity =>
			{
				entity.HasKey(e => e.StatusId);
				entity.Property(e => e.StatusMessage).IsRequired();
			});

			modelBuilder.Entity<ActualTask>(entity =>
			{
				entity.HasKey(e => e.TaskId);

				entity.HasOne(e => e.Status)
					.WithMany(s => s.tasks)
					.HasForeignKey(e => e.StatusId)
					.IsRequired(); 
			});

			modelBuilder.Entity<Priority>(entity =>
			{
				entity.HasKey(e => e.PriorityId);
				entity.Property(e => e.PriorityMessage).IsRequired();
			});

			modelBuilder.Entity<ActualTask>(entity =>
			{
				entity.HasOne(e => e.Priority)
					.WithMany(s => s.tasks)
					.HasForeignKey(e => e.PriorityId)
					.IsRequired();
			});

			modelBuilder.Entity<Notification>(entity =>
			{
				entity.HasOne(n => n.User)
					.WithMany(u => u.Notifications) 
					.HasForeignKey(n => n.UserId)
					.IsRequired();
			});
		}
	}
}

