using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Entities.Notifications;
using Engzly.Domain.Entities.Reviews;
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
        public DbSet<Review> Reviews { get; private set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(EngzlyDbContext).Assembly);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        }
    }
}