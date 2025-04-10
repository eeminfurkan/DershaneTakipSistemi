using System; // DateTime kullanmak için bu gerekli olabilir

namespace DershaneTakipSistemi.Models // Namespace'in proje adınla eşleştiğinden emin ol
{
    public class Ogrenci
    {
        public int Id { get; set; } // Primary Key olacak (EF Core bunu otomatik anlar)
        public string AdSoyad { get; set; }
        public string? TCKimlik { get; set; } // Null olabilir olarak işaretleyelim (?)
        public DateTime KayitTarihi { get; set; }

        // İleride eklenebilecek diğer özellikler:
        // public string Telefon { get; set; }
        // public string Email { get; set; }
        // public DateTime DogumTarihi { get; set; }
        // public string Adres { get; set; }
        // public bool AktifMi { get; set; } = true; // Varsayılan olarak aktif
    }
}