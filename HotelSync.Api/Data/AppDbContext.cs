using Microsoft.EntityFrameworkCore;
using HotelSync.Domain;

namespace HotelSync.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RateUpdate> RateUpdateLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Resolvendo o Warning: Definindo precisão para valores monetários
            modelBuilder.Entity<RateUpdate>()
                .Property(r => r.NewPrice)
                .HasColumnType("decimal(18,2)");

            // Mantendo nossa trava de Idempotência
            modelBuilder.Entity<RateUpdate>()
                .HasIndex(r => r.IdempotencyKey)
                .IsUnique();
                
            base.OnModelCreating(modelBuilder);
        }
    }
}
