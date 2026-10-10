using EkubApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EkubApi.Data;

public class EkubDbContext : DbContext
{
    public EkubDbContext(DbContextOptions<EkubDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<PhoneVerification> PhoneVerifications => Set<PhoneVerification>();
    public DbSet<Circle> Circles => Set<Circle>();
    public DbSet<CircleMember> CircleMembers => Set<CircleMember>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<SuccessStory> SuccessStories => Set<SuccessStory>();
    public DbSet<AccountDeletionRequest> AccountDeletionRequests => Set<AccountDeletionRequest>();
    public DbSet<EkubCategory> EkubCategories => Set<EkubCategory>();
    public DbSet<EkubSubCategory> EkubSubCategories => Set<EkubSubCategory>();
    public DbSet<EkubSubscription> EkubSubscriptions => Set<EkubSubscription>();
    public DbSet<CircleJoinRequest> CircleJoinRequests => Set<CircleJoinRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.HasIndex(u => u.PhoneNumber).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Gender).HasConversion<int>();
            entity.Property(u => u.JobType).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Location).IsRequired().HasMaxLength(200);
            entity.Property(u => u.ProfilePictureUrl).HasMaxLength(500);
            entity.Property(u => u.PinHash).HasMaxLength(256);
            entity.Property(u => u.PreferredLanguage).HasConversion<int>();
            entity.Property(u => u.ReferralCode).IsRequired().HasMaxLength(20);
            entity.HasIndex(u => u.ReferralCode).IsUnique();

            entity.HasOne(u => u.ReferredBy)
                .WithMany()
                .HasForeignKey(u => u.ReferredById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PhoneVerification
        modelBuilder.Entity<PhoneVerification>(entity =>
        {
            entity.HasKey(pv => pv.Id);
            entity.Property(pv => pv.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(pv => pv.VerificationCode).IsRequired().HasMaxLength(6);
            entity.HasOne(pv => pv.User)
                .WithMany()
                .HasForeignKey(pv => pv.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Circle
        modelBuilder.Entity<Circle>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Contribution).IsRequired().HasPrecision(18, 2);
            entity.Property(c => c.MeetingLabel).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Status).HasConversion<int>();

            entity.HasOne(c => c.Organizer)
                .WithMany(u => u.OrganizedCircles)
                .HasForeignKey(c => c.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Category)
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        // CircleMember
        modelBuilder.Entity<CircleMember>(entity =>
        {
            entity.HasKey(cm => cm.Id);

            entity.HasOne(cm => cm.Circle)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.CircleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cm => cm.User)
                .WithMany(u => u.CircleMemberships)
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(cm => new { cm.CircleId, cm.UserId }).IsUnique();
            entity.Property(cm => cm.PayoutOrder).HasDefaultValue(0);
        });

        // Round
        modelBuilder.Entity<Round>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Status).HasConversion<int>();

            entity.HasOne(r => r.Circle)
                .WithMany(c => c.Rounds)
                .HasForeignKey(r => r.CircleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Receiver)
                .WithMany()
                .HasForeignKey(r => r.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => new { r.CircleId, r.RoundNumber }).IsUnique();
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.LateFine).HasPrecision(18, 2).HasDefaultValue(0m);

            entity.HasOne(p => p.Round)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.RoundId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => new { p.RoundId, p.UserId }).IsUnique();
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Body).IsRequired().HasMaxLength(1000);
            entity.Property(n => n.Type).HasConversion<int>();

            entity.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Feedback
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Subject).IsRequired().HasMaxLength(200);
            entity.Property(f => f.Message).IsRequired().HasMaxLength(2000);
            entity.Property(f => f.Status).HasConversion<int>();

            entity.HasOne(f => f.User)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SuccessStory
        modelBuilder.Entity<SuccessStory>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.AuthorName).IsRequired().HasMaxLength(150);
            entity.Property(s => s.Content).IsRequired().HasMaxLength(2000);
            entity.Property(s => s.Rating).HasDefaultValue(5);

            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // AccountDeletionRequest
        modelBuilder.Entity<AccountDeletionRequest>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.VerificationCode).IsRequired().HasMaxLength(6);

            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EkubCategory
        modelBuilder.Entity<EkubCategory>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.Property(c => c.IconUrl).HasMaxLength(500);
        });

        // EkubSubCategory
        modelBuilder.Entity<EkubSubCategory>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.DailyContribution).HasPrecision(18, 2);
            entity.Property(s => s.TotalAmount).HasPrecision(18, 2);
            entity.Property(s => s.TermsAndConditions).IsRequired();
            entity.Property(s => s.Status).HasConversion<int>();

            entity.HasOne(s => s.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.CreatedByAdmin)
                .WithMany()
                .HasForeignKey(s => s.CreatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Circle)
                .WithMany()
                .HasForeignKey(s => s.CircleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EkubSubscription
        modelBuilder.Entity<EkubSubscription>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.SubCategory)
                .WithMany(sc => sc.Subscriptions)
                .HasForeignKey(s => s.SubCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // A user can only join a sub-category once
            entity.HasIndex(s => new { s.UserId, s.SubCategoryId }).IsUnique();

            entity.Property(s => s.Status).HasConversion<int>();
            entity.Property(s => s.FullName).HasMaxLength(150);
            entity.Property(s => s.NationalIdFan).HasMaxLength(50);
        });

        // CircleJoinRequest
        modelBuilder.Entity<CircleJoinRequest>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.HasOne(r => r.Circle)
                .WithMany(c => c.JoinRequests)
                .HasForeignKey(r => r.CircleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(r => r.Status).HasConversion<int>();
            entity.Property(r => r.Message).HasMaxLength(500);
        });
    }
}
