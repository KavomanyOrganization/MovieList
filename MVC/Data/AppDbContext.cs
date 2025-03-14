using MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MVC.Data;
public class AppDbContext: IdentityDbContext<Users>
{
    public DbSet<Movie> Movies { get; set; }
    public AppDbContext(DbContextOptions options): base(options){ }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>();
    }

}
