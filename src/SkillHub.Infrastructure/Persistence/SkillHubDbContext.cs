using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Persistence;

public sealed class SkillHubDbContext(DbContextOptions<SkillHubDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("AspNetUsers");
            entity.Property(user => user.FullName).HasMaxLength(200).IsRequired();
        });

        builder.Entity<Participant>(entity =>
        {
            entity.ToTable("Participants");
            entity.HasKey(participant => participant.Id);
            entity.Property(participant => participant.FullName).HasMaxLength(200).IsRequired();
            entity.Property(participant => participant.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(participant => participant.Email).IsUnique();
            entity.HasIndex(participant => participant.UserId).IsUnique();
        });

        builder.Entity<Instructor>(entity =>
        {
            entity.ToTable("Instructors");
            entity.HasKey(instructor => instructor.Id);
            entity.Property(instructor => instructor.FullName).HasMaxLength(200).IsRequired();
            entity.Property(instructor => instructor.Email).HasMaxLength(256).IsRequired();
            entity.Property(instructor => instructor.Specialty).HasMaxLength(150).IsRequired();
            entity.HasIndex(instructor => instructor.Email).IsUnique();
            entity.HasIndex(instructor => instructor.UserId).IsUnique();
        });

        builder.Entity<Workshop>(entity =>
        {
            entity.ToTable("Workshops", table =>
            {
                table.HasCheckConstraint("CK_Workshops_Capacity_Positive", "\"Capacity\" > 0");
                table.HasCheckConstraint("CK_Workshops_Schedule_Valid", "\"EndAt\" > \"StartAt\"");
            });
            entity.HasKey(workshop => workshop.Id);
            entity.Property(workshop => workshop.Code).HasMaxLength(40).IsRequired();
            entity.Property(workshop => workshop.Title).HasMaxLength(200).IsRequired();
            entity.Property(workshop => workshop.Description).HasMaxLength(2000).IsRequired();
            entity.HasIndex(workshop => workshop.Code).IsUnique();
            entity.HasIndex(workshop => workshop.StartAt);
            entity.HasOne(workshop => workshop.Instructor)
                .WithMany()
                .HasForeignKey(workshop => workshop.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollments", table =>
            {
                table.HasCheckConstraint("CK_Enrollments_Status_Valid", "\"Status\" IN ('Active', 'Cancelled')");
            });
            entity.HasKey(enrollment => enrollment.Id);
            entity.Property(enrollment => enrollment.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(enrollment => enrollment.CancellationReason).HasMaxLength(500);
            entity.HasIndex(enrollment => new { enrollment.ParticipantId, enrollment.WorkshopId })
                .IsUnique()
                .HasFilter("\"Status\" = 'Active'");
            entity.HasIndex(enrollment => enrollment.WorkshopId);
            entity.HasOne(enrollment => enrollment.Participant)
                .WithMany()
                .HasForeignKey(enrollment => enrollment.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(enrollment => enrollment.Workshop)
                .WithMany(workshop => workshop.Enrollments)
                .HasForeignKey(enrollment => enrollment.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
