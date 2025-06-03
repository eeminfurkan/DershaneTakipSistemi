using System;
using System.Collections.Generic; // ICollection için (ileride öğrenciler için)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // ForeignKey için


namespace DershaneTakipSistemi.Models
{
    public class Sinif
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Sınıf adı zorunludur.")]
        [Display(Name = "Sınıf Adı / Kodu")]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty; // Örn: "10-A Matematik", "YKS Hazırlık Grubu 1"

        [Display(Name = "Açıklama")]
        [DataType(DataType.MultilineText)]
        public string? Aciklama { get; set; }

        [Display(Name = "Kapasite")]
        [Range(1, 200, ErrorMessage = "Kapasite 1 ile 200 arasında olmalıdır.")] // Örnek bir aralık
        public int? Kapasite { get; set; } // Null olabilir

       // ===== YENİ EKLENEN ÖĞRETMEN İLİŞKİ ALANLARI =====
        [Display(Name = "Sınıf Öğretmeni")]
        public int? OgretmenPersonelId { get; set; } // Foreign Key (Personel tablosuna) - Nullable

        [Display(Name = "Sınıf Öğretmeni")]

        [ForeignKey("OgretmenPersonelId")]
        public virtual Personel? SorumluOgretmen { get; set; } // Navigation Property - Nullable
        // =============================================

        public virtual ICollection<Ogrenci>? Ogrenciler { get; set; }

        // İleride bu sınıftaki öğrencileri listelemek için (şimdilik yoruma alalım veya ekleyebiliriz)
        // public virtual ICollection<Ogrenci>? Ogrenciler { get; set; }
    }
}