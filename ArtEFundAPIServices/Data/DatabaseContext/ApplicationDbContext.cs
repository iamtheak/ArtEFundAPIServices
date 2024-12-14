using ArtEFundAPIServices.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace ArtEFundAPIServices.Data.DatabaseContext;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserModel> Users { get; set; }
    public DbSet<RoleModel> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RefreshTokenModel> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.UserModel)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.RoleModel)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<RefreshTokenModel>()
            .HasOne(rtm => rtm.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rtm => rtm.UserId);

        
        // Add unique constraint for Username
        modelBuilder.Entity<UserModel>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        // Add unique constraint for Email
        modelBuilder.Entity<UserModel>()
            .HasIndex(u => u.Email)
            .IsUnique();
        

        // Seed initial roles
        modelBuilder.Entity<RoleModel>().HasData(
            new RoleModel { RoleId = 1, RoleName = "admin" },
            new RoleModel { RoleId = 2, RoleName = "user" },
            new RoleModel { RoleId = 3, RoleName = "creator"}
        );
    }

    // Ensure database is created if it doesn't exist
    public void EnsureDatabaseCreated()
    {
        Database.EnsureCreated();
    }
}