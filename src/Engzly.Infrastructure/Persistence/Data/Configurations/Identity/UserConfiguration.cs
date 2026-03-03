using Engzly.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.UserConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {


            //builder.OwnsOne(loc => loc.Location, l =>
            //{
            //    l.Property(p => p.Latitude).HasColumnName("Latitude").IsRequired();
            //    l.Property(p => p.Longitude).HasColumnName("Longitude").IsRequired();
            //});
        }
    }
}

