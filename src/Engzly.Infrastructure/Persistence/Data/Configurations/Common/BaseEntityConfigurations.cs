using Engzly.Domain.Entities;
using Engzly.Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.Common;

public class BaseEntityConfigurations<TEntity, TKey> : IEntityTypeConfiguration<TEntity>
where TEntity : BaseEntity<TKey>
where TKey : IEquatable<TKey>
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();
    }
}