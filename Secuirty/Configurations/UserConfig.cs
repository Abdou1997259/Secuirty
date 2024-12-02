using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Secuirty.Models;

namespace Secuirty.Configurations
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.FirstName).IsRequired().HasColumnType("nvarchar(34)"); ;
            builder.Property(x => x.LastName).IsRequired().HasColumnType("nvarchar(34)");
            builder.Property(x => x.Email).IsRequired().HasColumnType("varchar(34)");
            builder.ToTable("User");


        }
    }
}
