﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DershaneTakipSistemi.Data;
using DershaneTakipSistemi.Models;
using Microsoft.AspNetCore.Authorization; // <-- Bu using gerekli!
using ClosedXML.Excel; // ClosedXML için
using System.IO;       // MemoryStream için
// Diğer mevcut using'ler...


namespace DershaneTakipSistemi.Controllers
{

    [Authorize(Roles = "Admin")] // <-- BU SATIRI EKLE

    public class OdemesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OdemesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Odemes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Odemeler.Include(o => o.Ogrenci); // Include eklendi!
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Odemes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Odemeler == null)
            {
                return NotFound();
            }

            // .Include(o => o.Ogrenci) ekle!
            var odeme = await _context.Odemeler
                .Include(o => o.Ogrenci)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (odeme == null)
            {
                return NotFound();
            }

            return View(odeme);
        }

        // GET: Odemes/Create
        public IActionResult Create()
        {
            // ViewData["OgrenciId"] = new SelectList(_context.Ogrenciler, "Id", "AdSoyad"); // <-- ESKİ SATIRI SİL
            OgrenciSelectListesiniYukle(); // <-- YENİ METODU ÇAĞIR (Parametresiz)
            return View();
        }

        // POST: Odemes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Odemes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(/*[Bind("Id,Tutar,OdemeTarihi,OgrenciId")]*/ Odeme odeme) // Bind'ı kaldırabiliriz
        {
            ModelState.Remove(nameof(odeme.Ogrenci)); // <-- BU SATIRI EKLE

            if (ModelState.IsValid)
            {
                _context.Add(odeme);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // ModelState geçersizse dropdown'ı tekrar yükle!
            OgrenciSelectListesiniYukle();
            return View(odeme);
        }

        // GET: Odemes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) // Sadece ID null mı diye kontrol etmek yeterli
            {
                return NotFound();
            }

            var odeme = await _context.Odemeler.FindAsync(id); // FindAsync kullanalım
            if (odeme == null)
            {
                return NotFound();
            }

            OgrenciSelectListesiniYukle(odeme.OgrenciId);
            return View(odeme);
        }

        // POST: Odemes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Odemes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Odeme odeme) // Bind attribute'unu kaldırmıştık
        {
            if (id != odeme.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(odeme.Ogrenci)); // Bu satır önemliydi

            if (ModelState.IsValid) // <<-- Bu blok içeriği önemli
            {
                try // <<-- Try bloğu
                {
                    _context.Entry(odeme).State = EntityState.Modified; // Güncelleme yöntemi
                    await _context.SaveChangesAsync(); // Kaydetme
                }
                catch (DbUpdateConcurrencyException) // <<-- Catch bloğu
                {
                    if (!OdemeExists(odeme.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index)); // <<-- Başarılı yönlendirme
            }

            // ModelState geçersizse dropdown'ı tekrar yükle
            OgrenciSelectListesiniYukle(odeme.OgrenciId);
            return View(odeme);
        }

        // GET: Odemes/Delete/5
        // GET: Odemes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Odemeler == null)
            {
                return NotFound();
            }

            // .Include(o => o.Ogrenci) ekle!
            var odeme = await _context.Odemeler
                .Include(o => o.Ogrenci)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (odeme == null)
            {
                return NotFound();
            }

            return View(odeme);
        }

        // POST: Odemes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var odeme = await _context.Odemeler.FindAsync(id);
            if (odeme != null)
            {
                _context.Odemeler.Remove(odeme);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OdemeExists(int id)
        {
            return _context.Odemeler.Any(e => e.Id == id);
        }

        private void OgrenciSelectListesiniYukle(object? seciliOgrenci = null)
        {
            // Öğrencileri çek ve Ad+Soyad içeren yeni bir anonim tip veya ViewModel oluştur.
            var ogrencilerListe = _context.Ogrenciler
                                        .OrderBy(o => o.Ad) // Ada göre sırala
                                        .ThenBy(o => o.Soyad) // Sonra Soyada göre sırala
                                        .Select(o => new { // Anonim tip oluştur
                                            Id = o.Id,
                                            TamAd = o.Ad + " " + o.Soyad // Ad ve Soyad'ı birleştir
                                        })
                                        .ToList(); // Listeye çevir

            // SelectList'i bu yeni liste üzerinden oluştur.
            // Gösterilecek metin alanı olarak "TamAd" kullan.
            ViewData["OgrenciId"] = new SelectList(ogrencilerListe, "Id", "TamAd", seciliOgrenci);
        }

        //GET: Odemes/ExportToExcel
        // ===== YENİ EKLENEN ÖDEME EXCEL EXPORT ACTION =====
        [HttpPost]
        public async Task<IActionResult> ExportOdemelerToExcel()
        {
            // 1. Veriyi Çekme (Öğrenci bilgisiyle birlikte ve sıralı)
            var odemeler = await _context.Odemeler
                                        .Include(o => o.Ogrenci) // İlişkili öğrenciyi getir
                                        .OrderByDescending(o => o.OdemeTarihi) // Ödeme tarihine göre tersten sırala
                                        .ToListAsync();

            // 2. Excel Dosyası Oluşturma
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ödemeler"); // "Ödemeler" adında bir sayfa

                // 3. Başlık Satırını Oluşturma
                int currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Ödeme ID";
                worksheet.Cell(currentRow, 2).Value = "Öğrenci Adı Soyadı";
                worksheet.Cell(currentRow, 3).Value = "Ödeme Tarihi";
                worksheet.Cell(currentRow, 4).Value = "Tutar";
                // İsterseniz Açıklama, Ödeme Tipi gibi alanları da ekleyebilirsiniz

                // Başlık satırını biçimlendirme (Opsiyonel)
                var headerRange = worksheet.Range($"A{currentRow}:D{currentRow}"); // Sütun sayısına göre ayarlayın
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightSkyBlue; // Farklı bir renk
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // 4. Veri Satırlarını Ekleme
                foreach (var odeme in odemeler)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = odeme.Id;
                    // Öğrenci null değilse adını soyadını yaz, null ise boş bırak (güvenlik önlemi)
                    worksheet.Cell(currentRow, 2).Value = odeme.Ogrenci != null ? $"{odeme.Ogrenci.Ad} {odeme.Ogrenci.Soyad}" : "Bilinmiyor";
                    worksheet.Cell(currentRow, 3).Value = odeme.OdemeTarihi.ToString("dd.MM.yyyy");
                    worksheet.Cell(currentRow, 4).Value = odeme.Tutar; // ClosedXML sayıyı otomatik formatlayabilir
                                                                       // Para birimi formatı için: worksheet.Cell(currentRow, 4).Value = odeme.Tutar;
                                                                       // worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0.00 ₺"; // Örnek format
                }

                // Sütun genişliklerini ayarlama (Opsiyonel)
                worksheet.Column(1).Width = 10; // Ödeme ID
                worksheet.Column(2).Width = 30; // Öğrenci Adı Soyadı
                worksheet.Column(3).Width = 15; // Ödeme Tarihi
                worksheet.Column(4).Width = 15; // Tutar
                                                // veya worksheet.Columns().AdjustToContents();

                // 5. Dosyayı MemoryStream'e Kaydetme
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    // 6. Dosyayı Tarayıcıya Gönderme
                    string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    string fileName = $"Odemeler_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                    return File(content, contentType, fileName);
                }
            }
        }
        // =================================================

        /*private void OgrenciSelectListesiniYukle(object? seciliOgrenci = null)
        {
            // Tüm öğrencileri veritabanından çekiyoruz.
            // .OrderBy(o => o.AdSoyad) ekleyerek listeyi isme göre sıralayabiliriz, daha kullanıcı dostu olur.
            var ogrencilerSorgusu = _context.Ogrenciler.OrderBy(o => o.AdSoyad);

            // SelectList'i oluşturuyoruz.
            // 1. parametre: Veri kaynağı (sıralanmış öğrenci listesi)
            // 2. parametre: Option'ların 'value' attribute'u için kullanılacak property adı (Öğrencinin Id'si)
            // 3. parametre: Option'ların kullanıcıya görünecek metni için kullanılacak property adı (AdSoyad)
            // 4. parametre: (Varsa) Hangi öğrencinin başlangıçta seçili olacağını belirten Id değeri.
            ViewData["OgrenciId"] = new SelectList(ogrencilerSorgusu, "Id", "AdSoyad", seciliOgrenci);
        }*/
    }
}
