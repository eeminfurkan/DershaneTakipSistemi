using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DershaneTakipSistemi.Data;
using DershaneTakipSistemi.Models;
using Microsoft.AspNetCore.Authorization; // <-- Bu using gerekli!


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
            if (id == null)
            {
                return NotFound();
            }

            var odeme = await _context.Odemeler
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
            // Öğrenci listesini veritabanından çek ve bir SelectList'e dönüştür.
            // SelectList(liste, değer_alanı_adı, gösterilecek_metin_alanı_adı)
            ViewData["OgrenciId"] = new SelectList(_context.Ogrenciler, "Id", "AdSoyad");
            // ViewData veya ViewBag kullanarak listeyi View'a gönderiyoruz.
            // "OgrenciId" anahtarını kullandık, çünkü View'daki <select> elementi buna bağlanacak.

            return View(); // Şimdi View'a liste gönderiliyor.
        }

        // POST: Odemes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tutar,OdemeTarihi,OgrenciId")] Odeme odeme)
        {
            if (ModelState.IsValid)
            {
                _context.Add(odeme);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(odeme);
        }

        // GET: Odemes/Edit/5
        // GET: Odemes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Odemeler == null)
            {
                return NotFound();
            }

            var odeme = await _context.Odemeler.FindAsync(id);
            if (odeme == null)
            {
                return NotFound();
            }

            // --- YENİ EKLENEN KISIM ---
            // Öğrenci listesini oluştur ve View'a gönder.
            // Önemli: SelectList'in 4. parametresi olarak mevcut ödemenin OgrenciId'sini veriyoruz ki
            // dropdown'da bu öğrenci seçili olarak gelsin.
            ViewData["OgrenciId"] = new SelectList(_context.Ogrenciler, "Id", "AdSoyad", odeme.OgrenciId);
            // -------------------------

            return View(odeme); // odeme nesnesini View'a göndermeye devam ediyoruz.
        }

        // POST: Odemes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Odemes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, /*[Bind("Id,Tutar,OdemeTarihi,OgrenciId")]*/ Odeme odeme)
        {
            if (id != odeme.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(odeme.Ogrenci));

            if (ModelState.IsValid)
            {
                try
                {
                    // _context.Update(odeme); // Eski satır
                    _context.Entry(odeme).State = EntityState.Modified; // Yeni satır
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OdemeExists(odeme.Id)) { return NotFound(); }
                    else { throw; }
                }
                return RedirectToAction(nameof(Index));
            }
            // ModelState geçersizse dropdown'ı tekrar doldur
            ViewData["OgrenciId"] = new SelectList(_context.Ogrenciler, "Id", "AdSoyad", odeme.OgrenciId);
            return View(odeme);
        }

        // GET: Odemes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var odeme = await _context.Odemeler
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
    }
}
