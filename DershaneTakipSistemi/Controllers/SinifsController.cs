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

    public class SinifsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SinifsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sinifs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Siniflar.ToListAsync());
        }

        // GET: Sinifs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinif = await _context.Siniflar
       .Include(s => s.Ogrenciler) // <-- BU SINIFTAKİ ÖĞRENCİLERİ GETİR
       .FirstOrDefaultAsync(m => m.Id == id);
            if (sinif == null)
            {
                return NotFound();
            }

            return View(sinif);
        }

        // GET: Sinifs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sinifs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ad,Aciklama,Kapasite")] Sinif sinif)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sinif);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sinif);
        }

        // GET: Sinifs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinif = await _context.Siniflar.FindAsync(id);
            if (sinif == null)
            {
                return NotFound();
            }
            return View(sinif);
        }

        // POST: Sinifs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Aciklama,Kapasite")] Sinif sinif)
        {
            if (id != sinif.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sinif);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SinifExists(sinif.Id))
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
            return View(sinif);
        }

        // GET: Sinifs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinif = await _context.Siniflar
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sinif == null)
            {
                return NotFound();
            }

            return View(sinif);
        }

        // POST: Sinifs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sinif = await _context.Siniflar.FindAsync(id);
            if (sinif != null)
            {
                _context.Siniflar.Remove(sinif);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SinifExists(int id)
        {
            return _context.Siniflar.Any(e => e.Id == id);
        }
    }
}
