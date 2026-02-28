using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Entities.Notifications;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Engzly.Infrastructure.Persistence.Data
{
    public class EngzlyDbContext(DbContextOptions<EngzlyDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<Gig> Gigs { get; private set; }
        public DbSet<GigAssignment> GigAssignments { get; private set; }
        public DbSet<Proposal> Proposals { get; private set; }
        public DbSet<Category> Categories { get; private set; }
        public DbSet<Notification> Notifications { get; private set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(u =>
            {
                u.OwnsOne(loc => loc.Location, l =>
                {
                    l.Property(p => p.Latitude).HasColumnName("Latitude").IsRequired();
                    l.Property(p => p.Longitude).HasColumnName("Longitude").IsRequired();
                });
            });

            builder.Entity<Gig>(gig =>
            {
                gig.HasOne(g => g.Client)
                   .WithMany()
                   .HasForeignKey(g => g.OwnerId)
                   .OnDelete(DeleteBehavior.Restrict);

                gig.HasOne(g => g.Category)
                   .WithMany()
                   .HasForeignKey(g => g.CategoryId);

                gig.OwnsOne(g => g.Location);
            });

            builder.Entity<GigAssignment>(ga =>
            {
                ga.HasKey(ga => new { ga.GigId, ga.TaskerId }); 

                ga.HasOne(ga => ga.Gig)
                  .WithMany()
                  .HasForeignKey(ga => ga.GigId);

                ga.HasOne(ga => ga.Tasker)
                  .WithMany()
                  .HasForeignKey(ga => ga.TaskerId);
            });


            builder.Entity<Proposal>(p =>
            {
                p.HasOne(p => p.Gig)
                 .WithMany()
                 .HasForeignKey(p => p.GigId);

                p.HasOne(p => p.Tasker)
                 .WithMany()
                 .HasForeignKey(p => p.TaskerId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}