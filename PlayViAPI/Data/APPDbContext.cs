using Microsoft.EntityFrameworkCore;
using PlayViAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace PlayViAPI.Data;

public class APPDbContext : IdentityDbContext<ApplicationUser>
{
    public APPDbContext(DbContextOptions<APPDbContext> options) : base(options) { }

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Title> Titles => Set<Title>();
    public DbSet<TitleGenre> TitleGenres => Set<TitleGenre>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>(); 

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        //subscription
        b.Entity<Subscription>()
        .HasOne<ApplicationUser>()
        .WithMany()
        .HasForeignKey(s => s.UserId);

        b.Entity<Subscription>().HasIndex(s => new { s.UserId, s.Status });

        //profile
        b.Entity<Profile>()
        .HasOne<ApplicationUser>()
        .WithMany()
        .HasForeignKey(p => p.UserId)
        .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Profile>().HasIndex(p => p.UserId);

        // Títulos de exemplo para testar (troque depois pelos seus)
        b.Entity<Title>().HasData(
            new Title { Id = 1, Name = "Comédia de Teste (antiga)", Type = "Movie", ReleaseYear = 1985, Description = "Título de teste.", VideoUrl = "https://exemplo.com/video1.mp4" },
            new Title { Id = 2, Name = "Comédia de Teste (nova)", Type = "Movie", ReleaseYear = 2023, Description = "Título de teste.", VideoUrl = "https://exemplo.com/video2.mp4" },
            new Title { Id = 3, Name = "Drama de Teste", Type = "Series", ReleaseYear = 2021, Description = "Título de teste.", VideoUrl = "https://exemplo.com/video3.mp4" }
        );
        
        b.Entity<TitleGenre>().HasData(
            new TitleGenre { TitleId = 1, GenreId = 1 },
            new TitleGenre { TitleId = 2, GenreId = 1 },
            new TitleGenre { TitleId = 3, GenreId = 2 }
        );

        b.Entity<TitleGenre>().HasKey(x => new { x.TitleId, x.GenreId });

        b.Entity<Plan>().Property(p => p.Price).HasPrecision(10, 2);

        // Dados iniciais
        b.Entity<Plan>().HasData(
            new Plan { Id = 1, Name = "Basic", Price = 19.90m, BillingPeriod = "Monthly" },
            new Plan { Id = 2, Name = "Standard", Price = 34.90m, BillingPeriod = "Monthly" },
            new Plan { Id = 3, Name = "Premium", Price = 349.00m, BillingPeriod = "Yearly" }
        );
        b.Entity<Genre>().HasData(
            new Genre { Id = 1, Name = "Comedy" },
            new Genre { Id = 2, Name = "Drama" },
            new Genre { Id = 3, Name = "Action" },
            new Genre { Id = 4, Name = "Horror" }
        );
    }
}