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
        public DbSet<Personel> Personeller { get; set; } // <-- YENİ EKLENEN SATIR


        // ===== YENİ EKLENEN/GÜNCELLENEN METOT =====
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Önce Identity yapılandırması çalışsın

            // Odeme entity'sindeki Tutar özelliği için hassasiyet ayarı
            modelBuilder.Entity<Odeme>() // Odeme entity'sini yapılandır
        .HasOne(o => o.Ogrenci) // Odeme'nin bir Ogrenci'si var
        .WithMany(p => p.Odemeler) // Ogrenci'nin birden çok Odeme'si var (Ogrenci'deki koleksiyon)
        .HasForeignKey(o => o.OgrenciId) // Yabancı anahtar Odeme'deki OgrenciId
        .OnDelete(DeleteBehavior.Restrict); // <-- SİLME DAVRANIŞINI AYARLA

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