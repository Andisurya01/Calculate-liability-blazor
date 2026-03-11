using Microsoft.AspNetCore.Mvc;
using IMSFinance.Models.ViewModels;
using IMSFinance.Services;

namespace IMSFinance.Controllers
{
    public class JatuhTempoAjaxRequest
    {
        public string KontrakNo { get; set; } = string.Empty;
        public DateTime PerTanggal { get; set; }
        public string Sumber { get; set; } = "db";
    }

    public class JatuhTempoController : Controller
    {
        private readonly IKreditService _dbService;
        private readonly IKreditJsonService _jsonService;
        private readonly ILogger<JatuhTempoController> _logger;

        public JatuhTempoController(
            IKreditService dbService,
            IKreditJsonService jsonService,
            ILogger<JatuhTempoController> logger)
        {
            _dbService   = dbService;
            _jsonService = jsonService;
            _logger      = logger;
        }

        // GET: /JatuhTempo
        public IActionResult Index()
        {
            return View(new JatuhTempoInputViewModel());
        }

        // POST: /JatuhTempo/CekDariDB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CekDariDB(JatuhTempoInputViewModel input)
        {
            if (!ModelState.IsValid)
                return View("Index", input);

            try
            {
                var result = await _dbService.GetJatuhTempoAsync(input.KontrakNo, input.PerTanggal);

                if (result == null)
                {
                    ModelState.AddModelError("", $"Kontrak '{input.KontrakNo}' tidak ditemukan di database.");
                    return View("Index", input);
                }

                ViewBag.Sumber = "Database";
                ViewBag.Input  = input;
                return View("Index", input);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error CekDariDB: {KontrakNo}", input.KontrakNo);
                ModelState.AddModelError("", "Gagal mengambil data dari database. Periksa koneksi.");
                return View("Index", input);
            }
        }

        // POST: /JatuhTempo/CekDariJson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CekDariJson(JatuhTempoInputViewModel input)
        {
            if (!ModelState.IsValid)
                return View("Index", input);

            try
            {
                var result = await _jsonService.GetJatuhTempoAsync(input.KontrakNo, input.PerTanggal);

                if (result == null)
                {
                    ModelState.AddModelError("", $"Kontrak '{input.KontrakNo}' tidak ditemukan di JSON.");
                    return View("Index", input);
                }

                ViewBag.Sumber = "JSON";
                ViewBag.Input  = input;
                ViewBag.Result = result;
                return View("Index", input);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error CekDariJson: {KontrakNo}", input.KontrakNo);
                ModelState.AddModelError("", "Gagal membaca data JSON.");
                return View("Index", input);
            }
        }

        // POST via AJAX untuk load result
        [HttpPost]
        public async Task<IActionResult> GetResult([FromBody] JatuhTempoAjaxRequest req)
        {
            try
            {
                JatuhTempoResultViewModel? result = req.Sumber == "json"
                    ? await _jsonService.GetJatuhTempoAsync(req.KontrakNo, req.PerTanggal)
                    : await _dbService.GetJatuhTempoAsync(req.KontrakNo, req.PerTanggal);

                if (result == null)
                    return NotFound(new { message = $"Kontrak '{req.KontrakNo}' tidak ditemukan." });

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetResult AJAX");
                return StatusCode(500, new { message = "Terjadi kesalahan server." });
            }
        }
    }
}
