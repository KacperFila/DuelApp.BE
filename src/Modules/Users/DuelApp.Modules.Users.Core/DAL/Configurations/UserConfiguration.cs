using DuelApp.Modules.Users.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuelApp.Modules.Users.Core.DAL.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Id).IsUnique();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}