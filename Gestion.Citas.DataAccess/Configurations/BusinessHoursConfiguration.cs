using Gestion.Citas.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gestion.Citas.DataAccess.Configurations
{
    public class BusinessHoursConfiguration : IEntityTypeConfiguration<BusinessHours>
    {
        public void Configure(EntityTypeBuilder<BusinessHours> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.DayOfWeek)
                .IsRequired();

            builder.Property(b => b.StartTime)
                .IsRequired();

            builder.Property(b => b.EndTime)
                .IsRequired();

            builder.Property(b => b.AppointmentDurationMin)
                .IsRequired();

            builder.HasOne(b => b.Doctor)
                .WithMany(d => d.WorkSchedule)
                .HasForeignKey(b => b.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(b => new { b.DoctorId, b.DayOfWeek })
                .IsUnique();
        }
    }
}
