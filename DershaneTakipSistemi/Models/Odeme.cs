using System; // DateTime ve decimal kullanmak için
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // HasPrecision için eklemiştik, [Column] için de gerekebilir.
namespace DershaneTakipSistemi.Models // Namespace'in proje adınla eşleştiğinden emin ol
{
    public class Odeme
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tutar alanı zorunludur.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Ödeme Tutarı")]
        public decimal Tutar { get; set; }

        [Required(ErrorMessage = "Ödeme Tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ödeme Tarihi")]
        public DateTime OdemeTarihi { get; set; }

        [Required(ErrorMessage = "Öğrenci seçimi zorunludur.")]
        [Display(Name = "Öğrenci")]
        [ForeignKey("Ogrenci")]
        public int OgrenciId { get; set; }

        // Navigation Property - Türkçe karakter sorunu olabilir, kontrol edin
        [Display(Name = "Öğrenci")]
        public virtual Ogrenci Ogrenci { get; set; }

        // İsteğe bağlı ek alanlar:
        // [Display(Name = "Açıklama")]
        // public string? Aciklama { get; set; } // Null olabilir
        // [Display(Name = "Ödeme Tipi")]
        // public string? OdemeTipi { get; set; } // Nakit, Kredi Kartı vb.

        // İleride eklenebilecek diğer özellikler:
        // public string Aciklama { get; set; }
        // public string OdemeTipi { get; set; } // Nakit, Kredi Kartı vb.

        // Navigation Property (İlişkiyi belirtmek için - İleri Seviye için):

    }
}