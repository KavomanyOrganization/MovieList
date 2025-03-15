using MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MVC.Data;
public class AppDbContext: IdentityDbContext<User>
{
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<MovieGenre> MovieGenres { get; set; }
    public DbSet<MovieCountry> MovieCountries { get; set; }
    public DbSet<UserMovie> UserMovies { get; set; }
    public DbSet<MovieCreator> MovieCreators { get; set; }
    public AppDbContext(DbContextOptions options): base(options){ }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>();
        modelBuilder.Entity<Country>();
        modelBuilder.Entity<Genre>();
        modelBuilder.Entity<Report>();

        modelBuilder.Entity<MovieGenre>()
            .HasKey(mg => new { mg.MovieId, mg.GenreId });

        modelBuilder.Entity<MovieGenre>()
            .HasOne(mg => mg.Movie)
            .WithMany(m => m.MovieGenres)
            .HasForeignKey(mg => mg.MovieId);

        modelBuilder.Entity<MovieGenre>()
            .HasOne(mg => mg.Genre)
            .WithMany(g => g.MovieGenres)
            .HasForeignKey(mg => mg.GenreId);

        modelBuilder.Entity<MovieCountry>()
            .HasKey(mg => new { mg.MovieId, mg.CountryId });

        modelBuilder.Entity<MovieCountry>()
            .HasOne(mg => mg.Movie)
            .WithMany(m => m.MovieCountries)
            .HasForeignKey(mg => mg.MovieId);

        modelBuilder.Entity<MovieCountry>()
            .HasOne(mg => mg.Country)
            .WithMany(g => g.MovieCountries)
            .HasForeignKey(mg => mg.CountryId);

        modelBuilder.Entity<UserMovie>()
            .HasKey(mg => new { mg.UserId, mg.MovieId });

        modelBuilder.Entity<UserMovie>()
            .HasOne(mg => mg.User)
            .WithMany(g => g.UserMovies)
            .HasForeignKey(mg => mg.UserId);

        modelBuilder.Entity<UserMovie>()
            .HasOne(mg => mg.Movie)
            .WithMany(m => m.UserMovies)
            .HasForeignKey(mg => mg.MovieId);

        modelBuilder.Entity<MovieCreator>()
            .HasKey(mg => new { mg.UserId, mg.MovieId });

        modelBuilder.Entity<MovieCreator>()
            .HasOne(mg => mg.User)
            .WithMany(g => g.MovieCreators)
            .HasForeignKey(mg => mg.UserId);

        modelBuilder.Entity<MovieCreator>()
            .HasOne(mg => mg.Movie)
            .WithMany(m => m.MovieCreators)
            .HasForeignKey(mg => mg.MovieId);
    }

}
