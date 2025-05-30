using Microsoft.EntityFrameworkCore;
 
namespace EduPlatform.Functions.Entities
{
    public partial class EduPlatformDbContext : DbContext
    {
        public EduPlatformDbContext()
        {
        }

        public EduPlatformDbContext(DbContextOptions<EduPlatformDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Course> Courses { get; set; }

        public virtual DbSet<CourseCategory> CourseCategories { get; set; }

        public virtual DbSet<Enrollment> Enrollments { get; set; }

        public virtual DbSet<Instructor> Instructors { get; set; }

        public virtual DbSet<Payment> Payments { get; set; }

        public virtual DbSet<Review> Reviews { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        public virtual DbSet<SessionDetail> SessionDetails { get; set; }

        public virtual DbSet<SmartApp> SmartApps { get; set; }

        public virtual DbSet<UserProfile> UserProfiles { get; set; }

        public virtual DbSet<UserRole> UserRoles { get; set; }

        public virtual DbSet<VideoRequest> VideoRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId).HasName("PK_Courses_CourseId");

                entity.Property(e => e.CourseType).HasMaxLength(10);
                entity.Property(e => e.Duration).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.Thumbnail).HasMaxLength(500);
                entity.Property(e => e.Title).HasMaxLength(100);

                entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Courses_CourseCategories");

                entity.HasOne(d => d.Instructor).WithMany(p => p.Courses)
                    .HasForeignKey(d => d.InstructorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Courses_Instructor");
            });

            modelBuilder.Entity<CourseCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId).HasName("PK_CourseCategories_CategoryId");

                entity.Property(e => e.CategoryName).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(250);
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.EnrollmentId).HasName("PK_Enrollments_EnrollmentId");

                entity.Property(e => e.EnrollmentDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.PaymentStatus).HasMaxLength(20);

                entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Enrollments_Courses");

                entity.HasOne(d => d.User).WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Enrollments_UserProfile");
            });

            modelBuilder.Entity<Instructor>(entity =>
            {
                entity.HasKey(e => e.InstructorId).HasName("PK_Instructors_InstructorId");

                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.HasOne(d => d.User).WithMany(p => p.Instructors)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Instructors_UserProfile");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId).HasName("PK_Payments_PaymentId");

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.PaymentDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.PaymentStatus).HasMaxLength(20);

                entity.HasOne(d => d.Enrollment).WithMany(p => p.Payments)
                    .HasForeignKey(d => d.EnrollmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Payments_Enrollments");
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.ReviewId).HasName("PK_Reviews_ReviewId");

                entity.Property(e => e.ReviewDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Course).WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Reviews_Courses");

                entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Reviews_UserProfile");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId).HasName("PK_Roles_RoleId");

                entity.Property(e => e.RoleName).HasMaxLength(50);
            });

            modelBuilder.Entity<SessionDetail>(entity =>
            {
                entity.HasKey(e => e.SessionId).HasName("PK_SessionDetails_SessionId");

                entity.Property(e => e.Title).HasMaxLength(100);
                entity.Property(e => e.VideoUrl).HasMaxLength(500);

                entity.HasOne(d => d.Course).WithMany(p => p.SessionDetails)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SessionDetails_Courses");
            });

            modelBuilder.Entity<SmartApp>(entity =>
            {
                entity.HasKey(e => e.SmartAppId).HasName("PK_SmartApp_SmartAppId");

                entity.ToTable("SmartApp");

                entity.Property(e => e.AppName).HasMaxLength(50);
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK_UserProfile_UserId");

                entity.ToTable("UserProfile");

                entity.Property(e => e.AdObjId).HasMaxLength(128);
                entity.Property(e => e.DisplayName)
                    .HasMaxLength(100)
                    .HasDefaultValue("Guest");
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.UserRoleId).HasName("PK_UserRole_UserRoleId");

                entity.ToTable("UserRole");

                entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRole_Roles");

                entity.HasOne(d => d.SmartApp).WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.SmartAppId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRole_SmartApp");

                entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRole_UserProfile");
            });

            modelBuilder.Entity<VideoRequest>(entity =>
            {
                entity.HasKey(e => e.VideoRequestId).HasName("PK_VideoRequests_VideoRequestId");

                entity.Property(e => e.RequestDescription).HasMaxLength(4000);
                entity.Property(e => e.Response).HasMaxLength(4000);
                entity.Property(e => e.ShortTitle).HasMaxLength(200);
                entity.Property(e => e.SubTopic).HasMaxLength(50);
                entity.Property(e => e.Topic).HasMaxLength(50);
                entity.Property(e => e.VideoUrls).HasMaxLength(2000);

                entity.HasOne(d => d.User).WithMany(p => p.VideoRequests)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VideoRequests_UserProfile");
            });

            OnModelCreatingPartial(modelBuilder);
        }
        
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
