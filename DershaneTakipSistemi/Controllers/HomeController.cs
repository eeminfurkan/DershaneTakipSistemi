using DershaneTakipSistemi.Data; // DbContext için
using DershaneTakipSistemi.Models; // ViewModel için
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // async metotlar ve CountAsync, SumAsync için
using System.Diagnostics; // ErrorViewModel için
using System.Linq; // Where, Sum vb. için
using System.Threading.Tasks; // Task için
using System; // DateTime için

namespace DershaneTakipSistemi.Controllers
{
    // HomeController'ý da yetkilendirebiliriz veya public býrakabiliriz.
    // Þimdilik public býrakalým ki giriþ yapmadan da ana sayfa görülebilsin.
    // [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // DbContext eklendi

        // Constructor güncellendi: ILogger yanýna ApplicationDbContext eklendi
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // Inject edilen DbContext atanýyor
        }

        // Index metodu async yapýldý ve ViewModel kullanacak þekilde güncellendi
        public async Task<IActionResult> Index()
        {
            // Baþlangýçta veritabaný boþ olabilir, null kontrolleri ekleyelim
            if (_context.Ogrenciler == null || _context.Odemeler == null)
            {
                // Veritabaný setleri null ise boþ bir model veya hata döndür
                // Þimdilik boþ view döndürelim veya basit bir ViewModel oluþturalým
                // return Problem("Veritabaný tablolarý bulunamadý.");
                return View(new DashboardViewModel()); // Boþ modelle View'ý döndür
            }


            // ViewModel nesnesini oluþtur
            var viewModel = new DashboardViewModel
            {
                // Toplam öðrenci sayýsýný asenkron olarak al
                ToplamOgrenciSayisi = await _context.Ogrenciler.CountAsync(),

                // Aktif öðrenci sayýsýný asenkron olarak al (AktifMi == true olanlar)
                AktifOgrenciSayisi = await _context.Ogrenciler.CountAsync(o => o.AktifMi),

                // Toplam ödeme tutarýný asenkron olarak al
                // Eðer hiç ödeme yoksa SumAsync hata verebilir, bu yüzden önce kontrol edelim veya DefaultIfEmpty kullanalým
                ToplamOdemeTutari = await _context.Odemeler.AnyAsync() ? await _context.Odemeler.SumAsync(p => p.Tutar) : 0,

                // Bugünkü ödeme sayýsýný asenkron olarak al
                BugunkuOdemeSayisi = await _context.Odemeler.CountAsync(p => p.OdemeTarihi.Date == DateTime.Today)
            };

            // Doldurulmuþ ViewModel'i View'a gönder
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}