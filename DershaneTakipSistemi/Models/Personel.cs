using System;
using System.ComponentModel.DataAnnotations; // Data Annotations için
using System.ComponentModel.DataAnnotations.Schema; // HasPrecision için eklemiştik, [Column] için de gerekebilir.


namespace DershaneTakipSistemi.Models
{
    public class Personel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Adı")]
        [StringLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyadı")]
        [StringLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Display(Name = "T.C. Kimlik No")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
        public string? TCKimlik { get; set; }

        [Display(Name = "Görevi")]
        [StringLength(100)]
        public string? Gorevi { get; set; }

        [Display(Name = "Cep Telefonu")]
        [StringLength(15)]
        public string? CepTelefonu { get; set; }

        [Display(Name = "E-Posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Display(Name = "İşe Başlama Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? IseBaslamaTarihi { get; set; } // Null olabilir

        [Display(Name = "Aktif Mi?")]
        public bool AktifMi { get; set; } = true; // Varsayılan olarak aktif

        public virtual ICollection<Sinif>? SorumluOlduguSiniflar { get; set; } // Navigation Property

        // AdSoyad birleşik göstermek için (Read-only property)
        [NotMapped] // Bu alan veritabanına maplenmeyecek
        [Display(Name = "Adı Soyadı")]
        public string AdSoyad => $"{Ad} {Soyad}";
    }
}