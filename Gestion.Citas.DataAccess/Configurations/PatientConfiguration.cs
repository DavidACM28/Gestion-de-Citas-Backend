using Gestion.Citas.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gestion.Citas.DataAccess.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.DocumentType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(p => p.DocumentNumber)
                .IsRequired()
                .HasMaxLength(50);
            builder.HasIndex(p => p.DocumentNumber).IsUnique();

            builder.Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.DateOfBirth)
                .IsRequired();

            builder.Property(p => p.PhoneNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Address)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
