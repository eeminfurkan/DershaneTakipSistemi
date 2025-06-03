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
    [Authorize(Roles = "Admin")]

    public class PersonelsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PersonelsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Personels
        public async Task<IActionResult> Index(string aramaMetniPersonel) // Parametre adı güncellendi
        {
            ViewData["GecerliAramaPersonel"] = aramaMetniPersonel; // ViewData anahtarı güncellendi

            var personellerSorgusu = from p in _context.Personeller select p;

            if (!String.IsNullOrEmpty(aramaMetniPersonel))
            {
                personellerSorgusu = personellerSorgusu.Where(p =>
                    (p.Ad != null && p.Ad.ToLower().Contains(aramaMetniPersonel.ToLower())) ||
                    (p.Soyad != null && p.Soyad.ToLower().Contains(aramaMetniPersonel.ToLower())) ||
                    (p.Gorevi != null && p.Gorevi.ToLower().Contains(aramaMetniPersonel.ToLower()))
                // İstersen TCKimlik veya diğer alanları da ekleyebilirsin
                );
            }

            var filtrelenmisPersoneller = await personellerSorgusu
                                                    .OrderBy(p => p.Ad)
                                                    .ThenBy(p => p.Soyad)
                                                    .ToListAsync();

            return View(filtrelenmisPersoneller);
        }

        // GET: Personels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personel = await _context.Personeller
                 .Include(p => p.SorumluOlduguSiniflar) // <-- SORUMLU OLDUĞU SINIFLARI GETİR
                .FirstOrDefaultAsync(m => m.Id == id);
            if (personel == null)
            {
                return NotFound();
            }

            return View(personel);
        }

        // GET: Personels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Personels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ad,Soyad,TCKimlik,Gorevi,CepTelefonu,Email,IseBaslamaTarihi,AktifMi")] Personel personel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(personel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(personel);
        }

        // GET: Personels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personel = await _context.Personeller.FindAsync(id);
            if (personel == null)
            {
                return NotFound();
            }
            return View(personel);
        }

        // POST: Personels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Soyad,TCKimlik,Gorevi,CepTelefonu,Email,IseBaslamaTarihi,AktifMi")] Personel personel)
        {
            if (id != personel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(personel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonelExists(personel.Id))
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
            return View(personel);
        }

        // GET: Personels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personel = await _context.Personeller
                .FirstOrDefaultAsync(m => m.Id == id);
            if (personel == null)
            {
                return NotFound();
            }

            return View(personel);
        }

        // POST: Personels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var personel = await _context.Personeller.FindAsync(id);
            if (personel != null)
            {
                _context.Personeller.Remove(personel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonelExists(int id)
        {
            return _context.Personeller.Any(e => e.Id == id);
        }

        // GET: Personels/ExportToExcel
        // ===== YENİ EKLENEN EXCEL EXPORT ACTION =====
        [HttpPost]
        public async Task<IActionResult> ExportPersonellerToExcel() // Metot adını uygun şekilde değiştirdik
        {
            // 1. Veriyi Çekme
            var personeller = await _context.Personeller
                                        .OrderBy(p => p.Ad)
                                        .ThenBy(p => p.Soyad)
                                        .ToListAsync();

            // 2. Excel Dosyası Oluşturma
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Personeller"); // Sayfa adı

                // 3. Başlık Satırını Oluşturma
                int currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "ID";
                worksheet.Cell(currentRow, 2).Value = "Adı";
                worksheet.Cell(currentRow, 3).Value = "Soyadı";
                worksheet.Cell(currentRow, 4).Value = "T.C. Kimlik No";
                worksheet.Cell(currentRow, 5).Value = "Görevi";
                worksheet.Cell(currentRow, 6).Value = "Telefon";
                worksheet.Cell(currentRow, 7).Value = "E-Posta";
                worksheet.Cell(currentRow, 8).Value = "İşe Başlama Tarihi";
                worksheet.Cell(currentRow, 9).Value = "Aktif Mi?";

                // Başlık satırını biçimlendirme (Opsiyonel)
                var headerRange = worksheet.Range($"A{currentRow}:I{currentRow}"); // Sütun sayısına göre ayarla
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen; // Farklı bir renk
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // 4. Veri Satırlarını Ekleme
                foreach (var personel in personeller)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = personel.Id;
                    worksheet.Cell(currentRow, 2).Value = personel.Ad;
                    worksheet.Cell(currentRow, 3).Value = personel.Soyad;
                    worksheet.Cell(currentRow, 4).Value = personel.TCKimlik;
                    worksheet.Cell(currentRow, 5).Value = personel.Gorevi;
                    worksheet.Cell(currentRow, 6).Value = personel.CepTelefonu;
                    worksheet.Cell(currentRow, 7).Value = personel.Email;
                    worksheet.Cell(currentRow, 8).Value = personel.IseBaslamaTarihi.HasValue ? personel.IseBaslamaTarihi.Value.ToString("dd.MM.yyyy") : "";
                    worksheet.Cell(currentRow, 9).Value = personel.AktifMi ? "Evet" : "Hayır";
                }

                // Sütun genişliklerini ayarlama (Opsiyonel)
                worksheet.Columns().AdjustToContents(); // İçeriğe göre otomatik ayarla

                // 5. Dosyayı MemoryStream'e Kaydetme
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    // 6. Dosyayı Tarayıcıya Gönderme
                    string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    string fileName = $"Personeller_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                    return File(content, contentType, fileName);
                }
            }
        }
    }
}
