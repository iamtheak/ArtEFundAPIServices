using ArtEFundAPIServices.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace ArtEFundAPIServices.Data.DatabaseContext;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /*
     *
     *  Added Users, Roles, Creators, Donations, Follows, Memberships, UserTypes, ContentTypes, Goals, RefreshTokens
     *
     *
     */
    public DbSet<UserModel> Users { get; set; }
    public DbSet<RoleModel> Roles { get; set; }
    public DbSet<CreatorModel> Creators { get; set; }
    public DbSet<DonationModel> Donations { get; set; }

    public DbSet<FollowModel> Follows { get; set; }
    public DbSet<MembershipModel> Memberships { get; set; }
    public DbSet<UserType> UserTypes { get; set; }
    public DbSet<ContentTypeModel> ContentTypes { get; set; }
    public DbSet<GoalModel> Goals { get; set; }
    public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
    public DbSet<EnrolledMembershipModel> EnrolledMembership { get; set; }

    public DbSet<GoalModel> GoalModels { get; set; }

    public DbSet<PostModel> Posts { get; set; }

    public DbSet<PostLikeModel> PostLikes { get; set; }

    public DbSet<PostCommentModel> PostComments { get; set; }

    public DbSet<PaymentModel> Payments { get; set; }

    public DbSet<CreatorApiKeyModel> CreatorApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<UserModel>()
            .HasOne(u => u.RoleModel)
            .WithMany()
            .HasForeignKey(u => u.RoleId);

        // Add unique constraint for Username
        modelBuilder.Entity<UserModel>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        // Add unique constraint for Email
        modelBuilder.Entity<UserModel>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<UserModel>()
            .HasOne(u => u.UserType)
            .WithMany()
            .HasForeignKey(um => um.UserTypeId);

        modelBuilder.Entity<CreatorModel>()
            .HasOne(cm => cm.UserModel)
            .WithOne()
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey<CreatorModel>(cm => cm.UserId);

        modelBuilder.Entity<CreatorModel>()
            .HasMany(cm => cm.Memberships)
            .WithOne()
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(m => m.CreatorId);

        modelBuilder.Entity<CreatorModel>()
            .HasOne(cm => cm.ContentType)
            .WithOne()
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey<CreatorModel>(cm => cm.ContentTypeId);

        modelBuilder.Entity<CreatorModel>()
            .HasMany(cm => cm.Goals)
            .WithOne()
            .HasForeignKey(g => g.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CreatorApiKeyModel>()
            .HasOne(capi => capi.Creator)
            .WithOne(c => c.ApiKey)
            .HasForeignKey<CreatorApiKeyModel>(capi => capi.CreatorId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<FollowModel>()
            .HasKey(f => new { f.UserId, f.CreatorId });

        modelBuilder.Entity<FollowModel>()
            .HasOne(f => f.User) // User follows a Creator
            .WithMany(u => u.Followings) // A User can follow multiple Creators
            .OnDelete(DeleteBehavior.Cascade)
            .HasForeignKey(f => f.UserId);

        modelBuilder.Entity<FollowModel>()
            .HasOne(f => f.Creator) // A Creator has multiple followers
            .WithMany(c => c.Followers) // A Creator can be followed by many Users
            .OnDelete(DeleteBehavior.Cascade)
            .HasForeignKey(f => f.CreatorId);

        modelBuilder.Entity<DonationModel>()
            .HasOne(d => d.Creator)
            .WithMany(c => c.Donations)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(d => d.CreatorId);

        modelBuilder.Entity<DonationModel>()
            .HasOne(d => d.Payment)
            .WithOne()
            .HasForeignKey<DonationModel>(d => d.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MembershipModel>()
            .HasOne(m => m.Creator)
            .WithMany(cm => cm.Memberships)
            .HasForeignKey(m => m.CreatorId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<EnrolledMembershipModel>()
            .HasOne(em => em.User)
            .WithMany(u => u.EnrolledMemberships)
            .HasForeignKey(em => em.UserId);

        modelBuilder.Entity<EnrolledMembershipModel>()
            .HasOne(em => em.Membership)
            .WithMany(m => m.EnrolledMemberships)
            .HasForeignKey(em => em.MembershipId);

        modelBuilder.Entity<EnrolledMembershipModel>()
            .HasOne(em => em.Payment)
            .WithOne()
            .HasForeignKey<EnrolledMembershipModel>(em => em.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<PostModel>()
            .HasMany(p => p.Likes)
            .WithOne(l => l.Post)
            .HasForeignKey(l => l.PostId);

        modelBuilder.Entity<PostModel>()
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId);

        modelBuilder.Entity<PostModel>()
            .Property(p => p.PostSlug)
            .IsRequired();


        modelBuilder.Entity<RoleModel>().HasData(
            new RoleModel { RoleId = 1, RoleName = "admin" },
            new RoleModel { RoleId = 2, RoleName = "user" },
            new RoleModel { RoleId = 3, RoleName = "creator" }
        );

        modelBuilder.Entity<UserType>().HasData(
            new UserType { UserTypeId = 1, UserTypeName = "credentials" },
            new UserType { UserTypeId = 2, UserTypeName = "google" }
        );

        modelBuilder.Entity<ContentTypeModel>().HasData(
            new ContentTypeModel { ContentTypeId = 1, ContentTypeName = "Infotainment" },
            new ContentTypeModel { ContentTypeId = 2, ContentTypeName = "Comedy" },
            new ContentTypeModel { ContentTypeId = 3, ContentTypeName = "Music" }
        );


        modelBuilder.Entity<DonationModel>()
            .Property(d => d.DonationAmount)
            .HasPrecision(18, 2); // 18 digits total, 2 decimal places

        modelBuilder.Entity<MembershipModel>()
            .Property(m => m.MembershipAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<GoalModel>()
            .Property(g => g.GoalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<GoalModel>()
            .Property(g => g.GoalProgress)
            .HasPrecision(18, 2);

        modelBuilder.Entity<EnrolledMembershipModel>()
            .Property(em => em.PaidAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PaymentModel>()
            .Property(pm => pm.Amount)
            .HasPrecision(18, 2);
    }

    public void EnsureDatabaseCreated()
    {
        Database.EnsureCreated();
    }
}