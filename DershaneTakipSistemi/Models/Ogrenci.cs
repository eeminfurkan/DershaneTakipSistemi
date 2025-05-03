using System; // DateTime kullanmak için bu gerekli olabilir
using System.Collections.Generic; // ICollection için gerekli
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // HasPrecision için eklemiştik, [Column] için de gerekebilir.
namespace DershaneTakipSistemi.Models // Namespace'in proje adınla eşleştiğinden emin ol
{
    public class Ogrenci
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Adı")]
        [StringLength(50)]
        public string Ad { get; set; } = string.Empty; // Null olmaması için başlangıç değeri atayalım

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyadı")]
        [StringLength(50)]
        public string Soyad { get; set; } = string.Empty; // Null olmaması için başlangıç değeri atayalım

        [Display(Name = "T.C. Kimlik No")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
        public string? TCKimlik { get; set; } // Null olabilir

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; } // Null olabilir

        [Display(Name = "Cep Telefonu")]
        [StringLength(15)]
        public string? CepTelefonu { get; set; } // Null olabilir

        [Display(Name = "E-Posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100)]
        public string? Email { get; set; } // Null olabilir

        [Display(Name = "Kayıt Tarihi")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Kayıt Tarihi zorunludur.")]
        public DateTime KayitTarihi { get; set; } = DateTime.Now; // Varsayılan değer

        [Display(Name = "Adres")]
        [DataType(DataType.MultilineText)]
        public string? Adres { get; set; } // Null olabilir

        [Display(Name = "Aktif Mi?")]
        public bool AktifMi { get; set; } = true; // Varsayılan değer

        [Display(Name = "Notlar")]
        [DataType(DataType.MultilineText)]
        public string? Notlar { get; set; } // Null olabilir


        // Navigation Property (Ödemeler için)
        public virtual ICollection<Odeme>? Odemeler { get; set; }

        // AdSoyad birleşik göstermek için (Read-only property)
        [NotMapped] // Bu alan veritabanına maplenmeyecek
        [Display(Name = "Adı Soyadı")]
        public string AdSoyad => $"{Ad} {Soyad}"; // Ad ve Soyad'ı birleştirir
    }
}