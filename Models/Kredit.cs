namespace IMSFinance.Models
{
    public class Kredit
    {
        public required string KontrakNo { get; set; }
        public required string ClientName { get; set; }
        public decimal OTR { get; set; }
        public decimal PersenDP { get; set; }
        public int Tenor { get; set; }
        public DateTime TanggalMulai { get; set; }

        public decimal TentukanBunga()
        {
            if (Tenor <= 12) return 0.12m;
            else if (Tenor <= 24) return 0.14m;
            else return 0.165m;
        }

        public decimal HitungDP() => OTR * (PersenDP / 100m);
        public decimal HitungPokok() => OTR - HitungDP();

        public decimal HitungAngsuran()
        {
            decimal pokok      = HitungPokok();
            decimal bunga      = TentukanBunga();
            decimal tahun      = Tenor / 12m;
            decimal totalBunga = pokok * bunga * tahun;
            return Math.Ceiling((pokok + totalBunga) / Tenor);
        }

        public List<JadwalAngsuran> GenerateJadwal()
        {
            var jadwal = new List<JadwalAngsuran>();
            for (int i = 1; i <= Tenor; i++)
            {
                jadwal.Add(new JadwalAngsuran
                {
                    KontrakNo         = KontrakNo,
                    AngsuranKe        = i,
                    AngsuranPerBulan  = HitungAngsuran(),
                    TanggalJatuhTempo = TanggalMulai.AddMonths(i - 1)
                });
            }
            return jadwal;
        }
    }

    public class JadwalAngsuran
    {
        public string KontrakNo { get; set; } = string.Empty;
        public int AngsuranKe { get; set; }
        public decimal AngsuranPerBulan { get; set; }
        public DateTime TanggalJatuhTempo { get; set; }
    }
}
