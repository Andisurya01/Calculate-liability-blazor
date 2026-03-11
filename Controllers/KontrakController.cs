using Microsoft.AspNetCore.Mvc;
using IMSFinance.Data;
using IMSFinance.Models;
using IMSFinance.Models.ViewModels;
namespace IMSFinance.Controllers
{
    public class KontrakController : Controller
    {
        private readonly DatabaseHelper _db;
        private readonly ILogger<KontrakController> _logger;

        public KontrakController(DatabaseHelper db, ILogger<KontrakController> logger)
        {
            _db     = db;
            _logger = logger;
        }

        // GET: /Kontrak
        public IActionResult Index()
        {
            return View(new KontrakInputViewModel());
        }

        // POST: /Kontrak/Hitung
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hitung(KontrakInputViewModel input)
        {
            if (!ModelState.IsValid)
                return View("Index", input);

            try
            {
                var kredit = new Kredit
                {
                    KontrakNo   = input.KontrakNo,
                    ClientName  = input.ClientName,
                    OTR         = input.OTR,
                    PersenDP    = input.PersenDP,
                    Tenor       = input.Tenor,
                    TanggalMulai = input.TanggalMulai
                };

                var jadwal = kredit.GenerateJadwal();

                // Simpan ke database
                await _db.SimpanKontrakAsync(kredit.KontrakNo, kredit.ClientName, kredit.OTR);
                await _db.SimpanJadwalAsync(jadwal.Select(j =>
                    (j.KontrakNo, j.AngsuranKe, j.AngsuranPerBulan, j.TanggalJatuhTempo)).ToList());

                var result = new KontrakResultViewModel
                {
                    Kredit = kredit,
                    Jadwal = jadwal
                };

                return View("Result", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghitung angsuran");
                ModelState.AddModelError("", "Terjadi kesalahan saat menyimpan data. Periksa koneksi database.");
                return View("Index", input);
            }
        }
    }
}
