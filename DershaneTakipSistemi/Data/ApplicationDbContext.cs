using DershaneTakipSistemi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DershaneTakipSistemi.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ogrenci> Ogrenciler { get; set; }
        public DbSet<Odeme> Odemeler { get; set; }

        // ===== YENİ EKLENEN/GÜNCELLENEN METOT =====
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Önce Identity yapılandırması çalışsın

            // Odeme entity'sindeki Tutar özelliği için hassasiyet ayarı
            modelBuilder.Entity<Odeme>()
                .Property(o => o.Tutar)
                .HasPrecision(18, 2);

            // Başka özel yapılandırmalar gerekirse buraya eklenebilir
            // Örneğin ilişki tanımları (ileride gerekirse):
            // modelBuilder.Entity<Odeme>()
            //     .HasOne(o => o.Ogrenci)
            //     .WithMany() // Öğrencinin birden çok ödemesi olabilir (WithMany parametresiz basit ilişki)
            //     .HasForeignKey(o => o.OgrenciId);
        }
        // ==========================================
    }
}