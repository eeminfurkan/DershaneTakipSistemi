using System; // DateTime ve decimal kullanmak için

namespace DershaneTakipSistemi.Models // Namespace'in proje adınla eşleştiğinden emin ol
{
    public class Odeme
    {
        public int Id { get; set; } // Primary Key
        public decimal Tutar { get; set; }
        public DateTime OdemeTarihi { get; set; }

        // Hangi öğrenciye ait olduğunu belirtmek için Foreign Key:
        public int OgrenciId { get; set; }

        // İleride eklenebilecek diğer özellikler:
        // public string Aciklama { get; set; }
        // public string OdemeTipi { get; set; } // Nakit, Kredi Kartı vb.

        // Navigation Property (İlişkiyi belirtmek için - İleri Seviye için):
        // public virtual Ogrenci Ogrenci { get; set; }
    }
}