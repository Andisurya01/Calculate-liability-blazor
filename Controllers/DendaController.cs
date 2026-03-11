using Microsoft.AspNetCore.Mvc;
using IMSFinance.Models.ViewModels;
using IMSFinance.Services;

namespace IMSFinance.Controllers
{
    public class DendaAjaxRequest
    {
        public string KontrakNo { get; set; } = string.Empty;
        public DateTime PerTanggal { get; set; }
        public int SudahBayarKe { get; set; }
        public string Sumber { get; set; } = "db";
    }

    public class DendaController : Controller
    {
        private readonly IKreditService _dbService;
        private readonly IKreditJsonService _jsonService;
        private readonly ILogger<DendaController> _logger;

        public DendaController(
            IKreditService dbService,
            IKreditJsonService jsonService,
            ILogger<DendaController> logger)
        {
            _dbService   = dbService;
            _jsonService = jsonService;
            _logger      = logger;
        }

        // GET: /Denda
        public IActionResult Index()
        {
            return View(new DendaInputViewModel());
        }

        // POST via AJAX
        [HttpPost]
        public async Task<IActionResult> GetResult([FromBody] DendaAjaxRequest req)
        {
            try
            {
                List<DendaItemViewModel> result = req.Sumber == "json"
                    ? await _jsonService.GetDendaAsync(req.KontrakNo, req.PerTanggal, req.SudahBayarKe)
                    : await _dbService.GetDendaAsync(req.KontrakNo, req.PerTanggal, req.SudahBayarKe);

                if (result.Count == 0)
                    return NotFound(new { message = $"Tidak ada denda untuk kontrak '{req.KontrakNo}'." });

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetDenda AJAX");
                return StatusCode(500, new { message = "Terjadi kesalahan server." });
            }
        }
    }
}
