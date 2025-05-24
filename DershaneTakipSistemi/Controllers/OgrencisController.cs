using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DershaneTakipSistemi.Data;
using DershaneTakipSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel; // ClosedXML için using eklendi
using System.IO; // MemoryStream için using eklendi
// Diğer using'ler (System, Linq, Task, Mvc, DbContext, Models, Authorization) zaten olmalı

namespace DershaneTakipSistemi.Controllers
{

    [Authorize(Roles = "Admin")] // <-- BU SATIRI EKLE

    public class OgrencisController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OgrencisController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ogrencis
        // Arama parametresi eklendi: string aramaMetni
        public async Task<IActionResult> Index(string aramaMetni)
        {
            // ViewData'ya mevcut arama metnini aktaralım ki View'da gösterilebilsin.
            ViewData["GecerliArama"] = aramaMetni;

            // Başlangıçta tüm öğrencileri seçelim.
            // IQueryable<Ogrenci> sorgusu oluşturuyoruz ki veritabanına sadece
            // en son filtrelenmiş sorgu gitsin.
            var ogrencilerSorgusu = from o in _context.Ogrenciler select o;
            // veya: var ogrencilerSorgusu = _context.Ogrenciler.AsQueryable();

            // Eğer aramaMetni boş değilse, filtreleme yap.
            if (!String.IsNullOrEmpty(aramaMetni))
            {
                // Ad veya Soyad alanında aramaMetni'ni içeren öğrencileri bul.
                // Büyük/küçük harf duyarsız arama için ToLower() kullanıyoruz.
                // EF Core 6+ string.Contains'i SQL LIKE '%...%' sorgusuna çevirir.
                ogrencilerSorgusu = ogrencilerSorgusu.Where(o =>
                    (o.Ad != null && o.Ad.ToLower().Contains(aramaMetni.ToLower())) ||
                    (o.Soyad != null && o.Soyad.ToLower().Contains(aramaMetni.ToLower()))
                // İstersen TCKimlik veya diğer alanları da ekleyebilirsin:
                // || (o.TCKimlik != null && o.TCKimlik.Contains(aramaMetni))
                );
            }
            // Son olarak, filtrelenmiş (veya filtrelenmemiş) sorguyu sıralayıp
            // veritabanından çekip View'a gönderelim.
            var filtrelenmisOgrenciler = await ogrencilerSorgusu
                                                .OrderBy(o => o.Ad)
                                                .ThenBy(o => o.Soyad)
                                                .ToListAsync();

            return View(filtrelenmisOgrenciler);
        }

        // GET: Ogrencis/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Öğrenciyi çekerken ilişkili Ödemeleri de getir:
            var ogrenci = await _context.Ogrenciler
                .Include(o => o.Odemeler) // <-- İLİŞKİLİ ÖDEMELERİ GETİR
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ogrenci == null)
            {
                return NotFound();
            }

            return View(ogrenci); // Artık 'ogrenci' nesnesi içinde Odemeler listesi de var (veya null)
        }

        // GET: Ogrencis/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ogrencis/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ogrenci ogrenci)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ogrenci);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ogrenci);
        }

        // GET: Ogrencis/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ogrenci = await _context.Ogrenciler.FindAsync(id);
            if (ogrenci == null)
            {
                return NotFound();
            }
            return View(ogrenci);
        }

        // POST: Ogrencis/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ogrenci ogrenci)
        {
            if (id != ogrenci.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ogrenci);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OgrenciExists(ogrenci.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ogrenci);
        }

        // GET: Ogrencis/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ogrenci = await _context.Ogrenciler
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ogrenci == null)
            {
                return NotFound();
            }

            return View(ogrenci);
        }

        // POST: Ogrencis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ogrenci = await _context.Ogrenciler.FindAsync(id);
            if (ogrenci == null) // Öğrenci bulunamadıysa NotFound dönelim
            {
                return NotFound();
            }

            try
            {
                _context.Ogrenciler.Remove(ogrenci);
                await _context.SaveChangesAsync(); // Hata potansiyeli olan satır try içinde
                TempData["SuccessMessage"] = "Öğrenci başarıyla silindi."; // Başarı mesajı (opsiyonel)
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) // EF Core veritabanı güncelleme hatası
            {
                // Hatanın nedenini anlamaya çalışalım (InnerException genellikle asıl hatayı içerir)
                // SQL Server'da Foreign Key hatası genellikle 547 numaralı hatadır.
                // Burada daha detaylı hata loglaması da yapılabilir.
                // Şimdilik genel bir mesaj verelim:

                // Silme işlemi başarısız olduğunda kullanıcıyı tekrar Delete onay sayfasına yönlendirelim
                // ve bir hata mesajı gösterelim.
                ModelState.AddModelError(string.Empty, "Bu öğrenci silinemedi. Öğrenciye ait ödeme kaydı olabilir. Lütfen önce ilişkili kayıtları silin.");

                // Kullanıcıya gösterilecek Delete onay sayfasını tekrar hazırlamamız gerekiyor.
                // Modelimizi (ogrenci nesnesi) tekrar View'a göndermeliyiz.
                return View("Delete", ogrenci); // "Delete" view'ını ogrenci modeliyle tekrar göster
            }
            catch (Exception ex) // Beklenmedik diğer hatalar için
            {
                ModelState.AddModelError(string.Empty, "Bir hata oluştu. Kayıt silinemedi.");
                // Loglama yapılabilir: _logger.LogError(ex, "Öğrenci silinirken hata oluştu");
                return View("Delete", ogrenci);
            }
        }

        private bool OgrenciExists(int id)
        {
            return _context.Ogrenciler.Any(e => e.Id == id);
        }

        //GET: Ogrencis/ExportToExcel
        // ===== YENİ EKLENEN EXCEL EXPORT ACTION =====
        [HttpPost] // Genellikle bu tür işlemler için POST kullanılır (GET de olabilir ama POST daha uygun)
        public async Task<IActionResult> ExportToExcel()
        {
            // 1. Veriyi Çekme (İstediğiniz filtrelemeyi veya sıralamayı burada yapabilirsiniz)
            var ogrenciler = await _context.Ogrenciler
                                        .OrderBy(o => o.Ad)
                                        .ThenBy(o => o.Soyad)
                                        .ToListAsync();

            // 2. Excel Dosyası Oluşturma (ClosedXML kullanarak)
            using (var workbook = new XLWorkbook()) // Yeni bir Excel çalışma kitabı oluştur
            {
                var worksheet = workbook.Worksheets.Add("Öğrenciler"); // "Öğrenciler" adında bir sayfa ekle

                // 3. Başlık Satırını Oluşturma
                int currentRow = 1; // Başlangıç satırı
                worksheet.Cell(currentRow, 1).Value = "ID"; // A1 hücresi
                worksheet.Cell(currentRow, 2).Value = "Adı"; // B1 hücresi
                worksheet.Cell(currentRow, 3).Value = "Soyadı"; // C1 hücresi
                worksheet.Cell(currentRow, 4).Value = "T.C. Kimlik No"; // D1 hücresi
                worksheet.Cell(currentRow, 5).Value = "Doğum Tarihi"; // E1 hücresi
                worksheet.Cell(currentRow, 6).Value = "Cep Telefonu"; // F1 hücresi
                worksheet.Cell(currentRow, 7).Value = "E-Posta"; // G1 hücresi
                worksheet.Cell(currentRow, 8).Value = "Kayıt Tarihi"; // H1 hücresi
                worksheet.Cell(currentRow, 9).Value = "Aktif Mi?"; // I1 hücresi
                // İsterseniz Adres ve Notlar'ı da ekleyebilirsiniz

                // Başlık satırını biçimlendirme (Opsiyonel)
                var headerRange = worksheet.Range($"A{currentRow}:I{currentRow}"); // Başlık aralığı (sütun sayısına göre ayarlayın)
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // 4. Veri Satırlarını Ekleme
                foreach (var ogr in ogrenciler)
                {
                    currentRow++; // Sonraki satıra geç
                    worksheet.Cell(currentRow, 1).Value = ogr.Id;
                    worksheet.Cell(currentRow, 2).Value = ogr.Ad;
                    worksheet.Cell(currentRow, 3).Value = ogr.Soyad;
                    worksheet.Cell(currentRow, 4).Value = ogr.TCKimlik;
                    // Tarihleri formatlı yazdıralım (Opsiyonel)
                    worksheet.Cell(currentRow, 5).Value = ogr.DogumTarihi.HasValue ? ogr.DogumTarihi.Value.ToString("dd.MM.yyyy") : "";
                    worksheet.Cell(currentRow, 6).Value = ogr.CepTelefonu;
                    worksheet.Cell(currentRow, 7).Value = ogr.Email;
                    worksheet.Cell(currentRow, 8).Value = ogr.KayitTarihi.ToString("dd.MM.yyyy");
                    worksheet.Cell(currentRow, 9).Value = ogr.AktifMi ? "Evet" : "Hayır";
                }

                // Sütun genişliklerini ayarlama (Opsiyonel)
                worksheet.Columns().AdjustToContents(); // İçeriğe göre otomatik ayarla

                // 5. Dosyayı MemoryStream'e Kaydetme
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream); // Çalışma kitabını stream'e kaydet
                    var content = stream.ToArray(); // Stream içeriğini byte dizisine çevir

                    // 6. Dosyayı Tarayıcıya Gönderme
                    string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    string fileName = $"Ogrenciler_{DateTime.Now:yyyyMMddHHmmss}.xlsx"; // Dinamik dosya adı

                    return File(content, contentType, fileName); // Dosyayı indir
                }
            }
        }
    }
}
