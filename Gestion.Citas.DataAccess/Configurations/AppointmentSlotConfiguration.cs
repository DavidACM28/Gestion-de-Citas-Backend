using Gestion.Citas.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gestion.Citas.DataAccess.Configurations
{
    public class AppointmentSlotConfiguration : IEntityTypeConfiguration<AppointmentSlot>
    {
        public void Configure(EntityTypeBuilder<AppointmentSlot> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Date)
                .IsRequired();

            builder.Property(s => s.Time)
                .IsRequired();

            builder.HasOne(s => s.Appointment)
                .WithMany(a => a.Slots)
                .HasForeignKey(s => s.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Doctor)
                .WithMany()
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => new { s.DoctorId, s.Date, s.Time })
                .IsUnique();
        }
    }
}
