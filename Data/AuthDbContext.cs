using JWTAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace JWTAuth;

public class AuthDbContext : DbContext
{
    private IConfiguration _configuration;
    public AuthDbContext(DbContextOptions<AuthDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<User> User { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        base.OnConfiguring(builder);
        builder.UseMySQL(_configuration.GetConnectionString("Default")!);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(x =>
        {
            x.HasKey(a => a.Id);
            x.Property(a => a.Id).ValueGeneratedOnAdd().HasMaxLength(128).IsRequired();
            x.Property(a => a.Username).HasMaxLength(256);
        });
    }

}
