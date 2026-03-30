using GoGoTour.Application.Abstractions;
using GoGoTour.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoGoTour.Infrastructure.Persistence;

public class GoGoTourDbContext : DbContext, IGoGoTourDbContext
{
    public GoGoTourDbContext(DbContextOptions<GoGoTourDbContext> options) : base(options)
    {
    }

    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<BookingRequest> BookingRequests => Set<BookingRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tour>(entity =>
        {
            entity.Property(t => t.Title).HasMaxLength(120).IsRequired();
            entity.Property(t => t.Country).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(2000).IsRequired();
            entity.Property(t => t.Price).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<BookingRequest>(entity =>
        {
            entity.Property(b => b.FullName).HasMaxLength(120).IsRequired();
            entity.Property(b => b.Email).HasMaxLength(180).IsRequired();
            entity.Property(b => b.Phone).HasMaxLength(50).IsRequired();
            entity.Property(b => b.Message).HasMaxLength(2000).IsRequired();

            entity.HasOne(b => b.Tour)
                .WithMany(t => t.BookingRequests)
                .HasForeignKey(b => b.TourId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Tour>().HasData(
            new Tour
            {
                Id = 1,
                Title = "Каппадокия на выходные",
                Country = "Турция",
                Description = "Полет на шарах, отель в скале и экскурсии по долинам.",
                Price = 499,
                DurationDays = 3,
                IsActive = true
            },
            new Tour
            {
                Id = 2,
                Title = "Северная Италия",
                Country = "Италия",
                Description = "Милан, Комо и гастрономический тур с локальным гидом.",
                Price = 1199,
                DurationDays = 6,
                IsActive = true
            });
    }
}
