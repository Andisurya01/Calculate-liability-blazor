using System.ComponentModel.DataAnnotations;
using IMSFinance.Models;

namespace IMSFinance.Models.ViewModels
{
    // ── Menu 1: Form Input Kontrak Baru ──
    public class KontrakInputViewModel
    {
        [Required(ErrorMessage = "Nomor kontrak wajib diisi")]
        [Display(Name = "Nomor Kontrak")]
        public string KontrakNo { get; set; } = "AGR00001";

        [Required(ErrorMessage = "Nama klien wajib diisi")]
        [Display(Name = "Nama Klien")]
        public string ClientName { get; set; } = string.Empty;

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "OTR harus lebih dari 0")]
        [Display(Name = "Harga OTR")]
        public decimal OTR { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "DP harus antara 1-100%")]
        [Display(Name = "Down Payment (%)")]
        public decimal PersenDP { get; set; } = 20;

        [Required]
        [Range(1, 360, ErrorMessage = "Tenor harus antara 1-360 bulan")]
        [Display(Name = "Tenor (Bulan)")]
        public int Tenor { get; set; } = 18;

        [Required]
        [Display(Name = "Tanggal Mulai Angsuran")]
        public DateTime TanggalMulai { get; set; } = new DateTime(2024, 1, 25);
    }

    public class KontrakResultViewModel
    {
        public Kredit Kredit { get; set; } = null!;
        public List<JadwalAngsuran> Jadwal { get; set; } = new();
    }

    // ── Menu 2: Cek Jatuh Tempo ──
    public class JatuhTempoInputViewModel
    {
        [Required(ErrorMessage = "Nomor kontrak wajib diisi")]
        [Display(Name = "Nomor Kontrak")]
        public string KontrakNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tanggal wajib diisi")]
        [Display(Name = "Per Tanggal")]
        public DateTime PerTanggal { get; set; } = DateTime.Today;
    }

    public class JatuhTempoResultViewModel
    {
        public string KontrakNo { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal TotalAngsuranJatuhTempo { get; set; }
        public List<JadwalAngsuran> DetailJadwal { get; set; } = new();
    }

    // ── Menu 3: Cek Denda ──
    public class DendaInputViewModel
    {
        [Required(ErrorMessage = "Nomor kontrak wajib diisi")]
        [Display(Name = "Nomor Kontrak")]
        public string KontrakNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tanggal wajib diisi")]
        [Display(Name = "Per Tanggal")]
        public DateTime PerTanggal { get; set; } = DateTime.Today;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Harus angka positif")]
        [Display(Name = "Sudah Bayar Sampai Angsuran Ke")]
        public int SudahBayarKe { get; set; } = 5;
    }

    public class DendaItemViewModel
    {
        public string KontrakNo { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public int AngsuranKe { get; set; }
        public int HariKeterlambatan { get; set; }
        public decimal TotalDenda { get; set; }
        public decimal AngsuranPerBulan { get; set; }
    }
}
