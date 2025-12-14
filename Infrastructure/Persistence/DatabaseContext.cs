namespace Infrastructure.Persistence;

using System.Reflection;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Pgvector;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options) {
  public DbSet<GameSystem> GameSystems { get; set; }

  protected override void OnModelCreating(ModelBuilder builder) {
    base.OnModelCreating(builder);
    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}

public class GameSystemConfig : IEntityTypeConfiguration<GameSystem> {

  public void Configure(EntityTypeBuilder<GameSystem> builder) {
    builder.ToTable($"{nameof(GameSystem)}s");
    builder.HasKey(g => g.Id);
    builder.Property(g => g.Id).ValueGeneratedNever();
    
    builder.HasMany(g => g.Chapters)
           .WithOne(c => c.GameSystem)
           .HasForeignKey(c => c.GameSystemId)
           .IsRequired();

    builder.Property(g => g.CharacterSheetSchema).HasColumnType("jsonb");
  }
}

public class RulebookChapterConfig : IEntityTypeConfiguration<RulebookChapter> {

  public void Configure(EntityTypeBuilder<RulebookChapter> builder) {
    builder.ToTable($"{nameof(RulebookChapter)}s");
    builder.HasKey(g => g.Id);
    builder.Property(g => g.Id).ValueGeneratedNever();
    builder.Property(g => g.SummaryEmbedding)
           .HasConversion(v => new Vector(v), v => v.ToArray())
           .HasColumnType("vector(3072)");
  }
}