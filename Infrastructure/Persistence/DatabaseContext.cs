namespace Infrastructure.Persistence;

using System.Data;
using System.Reflection;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using Pgvector;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options) {
  public DbSet<GameSystem> GameSystems { get; set; }
  public DbSet<RulebookChapter> RulebookChapters { get; set; }
  public DbSet<Campaign> Campaigns { get; set; }
  public DbSet<Character> Characters { get; set; }
  public DbSet<User> Users { get; set; }

  protected override void OnModelCreating(ModelBuilder builder) {
    base.OnModelCreating(builder);
    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    builder.HasPostgresExtension("vector");
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
           .HasColumnType("vector(1024)")
           .Metadata.SetValueComparer(new ValueComparer<float[]>(
             (v1, v2) => v1.SequenceEqual(v2),
             v => v.GetHashCode()       
           ));
    
    builder.Property<Vector>($"{nameof(RulebookChapter.SummaryEmbedding)}{nameof(Vector)}").HasColumnName(nameof(RulebookChapter.SummaryEmbedding));
    
    builder.HasIndex(c => c.SummaryEmbedding)
           .HasMethod("hnsw")
           .HasOperators("vector_cosine_ops");
  }
}

public class CampaignConfig : IEntityTypeConfiguration<Campaign> {

  public void Configure(EntityTypeBuilder<Campaign> builder) {
    builder.ToTable($"{nameof(Campaign)}s");
    builder.HasKey(g => g.Id);
    builder.Property(g => g.Id).ValueGeneratedNever();

    builder.HasOne(g => g.Owner)
           .WithMany(g => g.Campaigns)
           .HasForeignKey(g => g.OwnerId)
           .IsRequired();
    
    builder.HasOne(g => g.GameSystem)
           .WithMany(g => g.Campaigns)
           .HasForeignKey(g => g.GameSystemId)
           .IsRequired();
    
    builder.HasMany(g => g.Characters)
           .WithOne(g => g.Campaign)
           .HasForeignKey(g => g.CampaignId)
           .IsRequired();

    builder.HasMany(g => g.Sessions)
           .WithOne(g => g.Campaign)
           .HasForeignKey(g => g.CampaignId)
           .IsRequired();
  }
}

public class SessionConfig : IEntityTypeConfiguration<Session> {

  public void Configure(EntityTypeBuilder<Session> builder) {
    builder.ToTable($"{nameof(Session)}s");
    builder.HasKey(g => g.Id);
    builder.Property(g => g.Id).ValueGeneratedNever();
    
    builder.Property(g => g.SummaryEmbedding)
           .HasConversion(v => new Vector(v), v => v.ToArray())
           .HasColumnType("vector(1024)")
           .Metadata.SetValueComparer(new ValueComparer<float[]>(
             (v1, v2) => v1.SequenceEqual(v2),
             v => v.GetHashCode()       
           ));
    
    builder.Property<Vector>($"{nameof(Session.SummaryEmbedding)}{nameof(Vector)}").HasColumnName(nameof(Session.SummaryEmbedding));
    
    builder.HasIndex(c => c.SummaryEmbedding)
           .HasMethod("hnsw")
           .HasOperators("vector_cosine_ops");
  }
}

public class CharacterConfig : IEntityTypeConfiguration<Character> {

  public void Configure(EntityTypeBuilder<Character> builder) {
    builder.ToTable($"{nameof(Character)}s");
    builder.HasKey(g => g.Id);
    builder.Property(g => g.Id).ValueGeneratedNever();
    builder.Property(g => g.State).HasColumnType("jsonb");
  }
}

public class UserConfig : IEntityTypeConfiguration<User> {

  public void Configure(EntityTypeBuilder<User> builder) {
    builder.ToTable($"{nameof(User)}s");
    builder.HasKey(u => u.Id);
  }
}