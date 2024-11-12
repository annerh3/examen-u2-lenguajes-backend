using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoExamenU2.Database.Entities;

namespace ProyectoExamenU2.Database.Configuration
{
    public class JournalEntryDetailConfiguraction : IEntityTypeConfiguration<JournalEntryDetailEntity>
    {
        public void Configure(EntityTypeBuilder<JournalEntryDetailEntity> builder)
        {
            builder.HasOne(e => e.CreatedByUser)
              .WithMany()
              .HasForeignKey(e => e.CreatedBy)
              .HasPrincipalKey(e => e.Id);
            //  .IsRequired();

            builder.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .HasPrincipalKey(e => e.Id);
            //  .IsRequired();
        }
    }
}
