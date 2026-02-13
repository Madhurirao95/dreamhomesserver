using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DREAMHOMES.Models.Repository.Db_Context;

public partial class DreamhomesContext : IdentityDbContext<ApplicationUser>
{
    public DreamhomesContext()
    {
    }

    public DreamhomesContext(DbContextOptions<DreamhomesContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<SellerInformation>()
            .Property(e => e.Location)
            .IsRequired();

        modelBuilder.Entity<SellerInformation>()
            .HasMany(d => d.Documents)
            .WithOne()
            .HasForeignKey(s => s.SellerId)
            .IsRequired();

        modelBuilder.Entity<Document>()
            .HasOne<SellerInformation>()
            .WithMany(d => d.Documents)
            .HasForeignKey(s => s.SellerId)
            .IsRequired();

        modelBuilder.Entity<SellerInformation>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .IsRequired();

        modelBuilder.Entity<ApplicationUser>()
            .HasMany<SellerInformation>()
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .IsRequired();

        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.Agent)
            .WithMany()
            .HasForeignKey(c => c.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Conversation>()
            .HasMany(e => e.Messages)
            .WithOne()
            .HasForeignKey(s => s.ConversationId)
            .IsRequired();

        modelBuilder.Entity<Conversation>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.Property(c => c.AgentId)
                  .IsRequired();
            entity.Property(c => c.UserId)
                  .IsRequired();
        });

        modelBuilder.Entity<ChatMessage>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.Property(c => c.Content)
                  .IsRequired();
            entity.Property(c => c.ConversationId)
                  .IsRequired();
            entity.Property(c => c.UserId)
                  .IsRequired();
        });


        modelBuilder.Entity<ChatMessage>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(c => c.Conversation)
            .WithMany(d => d.Messages)
            .HasForeignKey(c => c.ConversationId)
            .IsRequired();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(NetTopologySuite.Geometries.Point) ||
                    property.ClrType == typeof(NetTopologySuite.Geometries.Geometry))
                {
                    if (Database.IsSqlServer())
                    {
                        property.SetColumnType("geography");
                    }
                    else if (Database.IsSqlite())
                    {
                        property.SetColumnType("GEOMETRY");
                    }
                }
            }
        }
    }

    public DbSet<SellerInformation> SellerInformation { get; set; }

    public DbSet<Document> Document { get; set; }

    public DbSet<Conversation> Conversations { get; set; }

    public DbSet<ChatMessage> ChatMessages { get; set; }
}
