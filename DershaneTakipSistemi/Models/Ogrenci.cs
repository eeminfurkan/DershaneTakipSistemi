using System; // DateTime kullanmak için bu gerekli olabilir
using System.Collections.Generic; // ICollection için gerekli
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // HasPrecision için eklemiştik, [Column] için de gerekebilir.
namespace DershaneTakipSistemi.Models // Namespace'in proje adınla eşleştiğinden emin ol
{
    public class Ogrenci
    {
         public int Id { get; set; }

        [Display(Name = "Adı Soyadı")]
        // Belki Ad ve Soyad'ı ayırmak daha iyi olabilir? Şimdilik böyle kalsın.
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")] // Zorunlu yapalım
        public string AdSoyad { get; set; }

        [Display(Name = "T.C. Kimlik No")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")] // Uzunluk kontrolü
        public string? TCKimlik { get; set; } // Null olabilir

        [Display(Name = "Kayıt Tarihi")]
        [DataType(DataType.Date)] // Tarih formatı
        [Required(ErrorMessage = "Kayıt Tarihi zorunludur.")] // Zorunlu yapalım
        public DateTime KayitTarihi { get; set; }

        public virtual ICollection<Odeme>? Odemeler { get; set; } // Null olabilir (?)


        // İleride eklenebilecek diğer özellikler:
        // public string Telefon { get; set; }
        // public string Email { get; set; }
        // public DateTime DogumTarihi { get; set; }
        // public string Adres { get; set; }
        // public bool AktifMi { get; set; } = true; // Varsayılan olarak aktif
    }
}