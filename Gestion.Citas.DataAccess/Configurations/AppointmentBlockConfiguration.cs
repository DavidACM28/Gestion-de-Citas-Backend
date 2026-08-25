using Gestion.Citas.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gestion.Citas.DataAccess.Configurations
{
    public class AppointmentBlockConfiguration : IEntityTypeConfiguration<AppointmentBlock>
    {
        public void Configure(EntityTypeBuilder<AppointmentBlock> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Date)
                .IsRequired();

            builder.Property(a => a.StartTime)
                .IsRequired();

            builder.Property(a => a.EndTime)
                .IsRequired();

            builder.Property(a => a.Reason)
                .IsRequired()
                .HasMaxLength(250);

            builder.HasOne(a => a.Doctor)
                .WithMany(d => d.blocks)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
