using System.Text.Json;
using IMSFinance.Models;
using IMSFinance.Models.ViewModels;

namespace IMSFinance.Services
{
    public class KreditJsonService : IKreditJsonService
    {
        private readonly string _jsonPath;
        private readonly ILogger<KreditJsonService> _logger;

        // Cache supaya tidak baca file berulang kali
        private List<KontrakJson>? _cache;

        public KreditJsonService(IWebHostEnvironment env, ILogger<KreditJsonService> logger)
        {
            _jsonPath = Path.Combine(env.WebRootPath, "data", "data.json");
            _logger   = logger;
        }

        private async Task<List<KontrakJson>> LoadDataAsync()
        {
            if (_cache != null) return _cache;

            try
            {
                await using var stream = File.OpenRead(_jsonPath);
                var root = await JsonSerializer.DeserializeAsync<JsonRoot>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                _cache = root?.Kontrak ?? new List<KontrakJson>();
                return _cache;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal membaca data.json");
                throw;
            }
        }

        public async Task<List<string>> GetAllKontrakNoAsync()
        {
            var data = await LoadDataAsync();
            return data.Select(k => k.KontrakNo).ToList();
        }

        public async Task<JatuhTempoResultViewModel?> GetJatuhTempoAsync(string kontrakNo, DateTime perTanggal)
        {
            var data    = await LoadDataAsync();
            var kontrak = data.FirstOrDefault(k =>
                k.KontrakNo.Equals(kontrakNo, StringComparison.OrdinalIgnoreCase));

            if (kontrak == null) return null;

            var jadwalJatuhTempo = kontrak.JadwalAngsuran
                .Where(j => j.TanggalJatuhTempo.Date <= perTanggal.Date)
                .ToList();

            return new JatuhTempoResultViewModel
            {
                KontrakNo               = kontrak.KontrakNo,
                ClientName              = kontrak.ClientName,
                TotalAngsuranJatuhTempo = jadwalJatuhTempo.Sum(j => j.AngsuranPerBulan),
                DetailJadwal            = jadwalJatuhTempo.Select(j => new JadwalAngsuran
                {
                    KontrakNo         = kontrak.KontrakNo,
                    AngsuranKe        = j.AngsuranKe,
                    AngsuranPerBulan  = j.AngsuranPerBulan,
                    TanggalJatuhTempo = j.TanggalJatuhTempo
                }).ToList()
            };
        }

        public async Task<List<DendaItemViewModel>> GetDendaAsync(string kontrakNo, DateTime perTanggal, int sudahBayarKe)
        {
            var data    = await LoadDataAsync();
            var kontrak = data.FirstOrDefault(k =>
                k.KontrakNo.Equals(kontrakNo, StringComparison.OrdinalIgnoreCase));

            if (kontrak == null) return new List<DendaItemViewModel>();

            return kontrak.JadwalAngsuran
                .Where(j => j.AngsuranKe > sudahBayarKe
                         && j.TanggalJatuhTempo.Date <= perTanggal.Date)
                .Select(j =>
                {
                    int hari = (perTanggal.Date - j.TanggalJatuhTempo.Date).Days;
                    return new DendaItemViewModel
                    {
                        KontrakNo         = kontrak.KontrakNo,
                        ClientName        = kontrak.ClientName,
                        AngsuranKe        = j.AngsuranKe,
                        AngsuranPerBulan  = j.AngsuranPerBulan,
                        HariKeterlambatan = hari,
                        TotalDenda        = Math.Round(j.AngsuranPerBulan * 0.001m * hari, 0)
                    };
                })
                .ToList();
        }

        // ── JSON mapping classes ──
        private class JsonRoot
        {
            public List<KontrakJson> Kontrak { get; set; } = new();
        }

        private class KontrakJson
        {
            public string KontrakNo { get; set; } = string.Empty;
            public string ClientName { get; set; } = string.Empty;
            public decimal Otr { get; set; }
            public List<JadwalJson> JadwalAngsuran { get; set; } = new();
        }

        private class JadwalJson
        {
            public int AngsuranKe { get; set; }
            public decimal AngsuranPerBulan { get; set; }
            public DateTime TanggalJatuhTempo { get; set; }
        }
    }
}
