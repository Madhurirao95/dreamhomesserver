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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-T30UDT3\\SQLEXPRESS;Database=DreamHomes;Trusted_Connection=True;TrustServerCertificate=True");

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

        modelBuilder.Entity<IdentityUser>()
            .HasMany<SellerInformation>()
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .IsRequired();
    }

    public DbSet<SellerInformation> SellerInformation { get; set; }

    public DbSet<Document> Document { get; set; }
}
