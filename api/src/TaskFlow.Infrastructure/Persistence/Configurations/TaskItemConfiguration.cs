using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    // SQLite has no native DateTimeOffset support and cannot sort it server-side. We persist
    // a UTC DateTime under the covers; the domain still works in DateTimeOffset everywhere else.
    // On a production database (PostgreSQL/SQL Server) this converter would be removed.
    private static readonly ValueConverter<DateTimeOffset, DateTime> UtcConverter =
        new(v => v.UtcDateTime, v => new DateTimeOffset(v, TimeSpan.Zero));

    private static readonly ValueConverter<DateTimeOffset?, DateTime?> NullableUtcConverter =
        new(v => v.HasValue ? v.Value.UtcDateTime : null,
            v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : null);

    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(TaskItem.MaxTitleLength);

        builder.Property(t => t.Description)
            .HasMaxLength(TaskItem.MaxDescriptionLength);

        builder.Property(t => t.Assignee)
            .HasMaxLength(TaskItem.MaxAssigneeLength);

        builder.Property(t => t.Status).HasConversion<int>();
        builder.Property(t => t.Priority).HasConversion<int>();

        builder.Property(t => t.CreatedAt).HasConversion(UtcConverter).IsRequired();
        builder.Property(t => t.UpdatedAt).HasConversion(UtcConverter).IsRequired();
        builder.Property(t => t.DueDate).HasConversion(NullableUtcConverter);

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.DueDate);
        builder.HasIndex(t => t.CreatedAt);
    }
}
