using Gestion.Citas.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gestion.Citas.DataAccess.Configurations
{
    public class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
    {
        public void Configure(EntityTypeBuilder<Specialty> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);
            builder.HasIndex(s => s.Name).IsUnique();

            builder.Property(s => s.Description)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
