using Engzly.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Engzly.Infrastructure.Persistence.Data
{
    public class EngzlyDbContext(DbContextOptions<EngzlyDbContext> options) : IdentityDbContext<User>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>(builder =>
            {
                builder.OwnsOne(u => u.Location, loc =>
                {
                    loc.Property(p => p.Latitude)
                       .HasColumnName("Latitude")
                       .IsRequired();

                    loc.Property(p => p.Longitude)
                       .HasColumnName("Longitude")
                       .IsRequired();
                });
            });

            base.OnModelCreating(builder);

        }
    }
}
