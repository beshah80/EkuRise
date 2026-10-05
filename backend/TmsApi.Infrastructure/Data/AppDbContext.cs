using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Domain.Enums;

namespace TmsApi.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(20);
            e.HasIndex(u => u.PhoneNumber).IsUnique();
            e.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            e.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            e.Property(u => u.Gender).HasConversion<int>();
            e.Property(u => u.JobType).IsRequired().HasMaxLength(100);
            e.Property(u => u.Location).IsRequired().HasMaxLength(200);
            e.Property(u => u.PinHash).HasMaxLength(256);
            e.Property(u => u.PreferredLanguage).HasConversion<int>();
            e.Property(u => u.ReferralCode).IsRequired().HasMaxLength(20);
            e.HasIndex(u => u.ReferralCode).IsUnique();
            e.HasOne(u => u.ReferredBy).WithMany().HasForeignKey(u => u.ReferredById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PhoneVerification>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.PhoneNumber).IsRequired().HasMaxLength(20);
            e.Property(p => p.VerificationCode).IsRequired().HasMaxLength(6);
            e.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Circle>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.Contribution).HasPrecision(18, 2);
            e.Property(c => c.MeetingLabel).IsRequired().HasMaxLength(50);
            e.Property(c => c.Status).HasConversion<int>();
            e.HasOne(c => c.Organizer).WithMany(u => u.OrganizedCircles).HasForeignKey(c => c.OrganizerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CircleMember>(e =>
        {
            e.HasKey(cm => cm.Id);
            e.HasOne(cm => cm.Circle).WithMany(c => c.Members).HasForeignKey(cm => cm.CircleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cm => cm.User).WithMany(u => u.CircleMemberships).HasForeignKey(cm => cm.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(cm => new { cm.CircleId, cm.UserId }).IsUnique();
            e.Property(cm => cm.PayoutOrder).HasDefaultValue(0);
        });

        modelBuilder.Entity<Round>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Status).HasConversion<int>();
            e.HasOne(r => r.Circle).WithMany(c => c.Rounds).HasForeignKey(r => r.CircleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Receiver).WithMany().HasForeignKey(r => r.ReceiverId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(r => new { r.CircleId, r.RoundNumber }).IsUnique();
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.LateFine).HasPrecision(18, 2).HasDefaultValue(0m);
            e.HasOne(p => p.Round).WithMany(r => r.Payments).HasForeignKey(p => p.RoundId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(p => new { p.RoundId, p.UserId }).IsUnique();
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Title).IsRequired().HasMaxLength(200);
            e.Property(n => n.Body).IsRequired().HasMaxLength(1000);
            e.Property(n => n.Type).HasConversion<int>();
            e.HasOne(n => n.User).WithMany(u => u.Notifications).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Feedback>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Subject).IsRequired().HasMaxLength(200);
            e.Property(f => f.Message).IsRequired().HasMaxLength(2000);
            e.Property(f => f.Status).HasConversion<int>();
            e.HasOne(f => f.User).WithMany(u => u.Feedbacks).HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SuccessStory>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.AuthorName).IsRequired().HasMaxLength(150);
            e.Property(s => s.Content).IsRequired().HasMaxLength(2000);
            e.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AccountDeletionRequest>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.VerificationCode).IsRequired().HasMaxLength(6);
            e.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EkubCategory>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<EkubSubCategory>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.Property(s => s.DailyContribution).HasPrecision(18, 2);
            e.Property(s => s.TotalAmount).HasPrecision(18, 2);
            e.Property(s => s.Status).HasConversion<int>();
            e.HasOne(s => s.Category).WithMany(c => c.SubCategories).HasForeignKey(s => s.CategoryId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.CreatedByAdmin).WithMany().HasForeignKey(s => s.CreatedByAdminId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Circle).WithMany().HasForeignKey(s => s.CircleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EkubSubscription>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.SubCategory).WithMany(sc => sc.Subscriptions).HasForeignKey(s => s.SubCategoryId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(s => new { s.UserId, s.SubCategoryId }).IsUnique();
        });
    }
}
