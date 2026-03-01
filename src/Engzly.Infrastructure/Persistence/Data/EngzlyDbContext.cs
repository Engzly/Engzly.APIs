using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Engzly.Infrastructure.Persistence.Data
{
    public class EngzlyDbContext(DbContextOptions<EngzlyDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<Gig> Gigs { get; set; }
        public DbSet<GigAssignment> GigAssignments { get; set; }
        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(EngzlyDbContext).Assembly);

        }
    }
}