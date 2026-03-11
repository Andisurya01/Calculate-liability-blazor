using Npgsql;
using IMSFinance.Models;
using IMSFinance.Models.ViewModels;

namespace IMSFinance.Services
{
    public class KreditDbService : IKreditService
    {
        private readonly string _connectionString;
        private readonly ILogger<KreditDbService> _logger;

        public KreditDbService(IConfiguration config, ILogger<KreditDbService> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string tidak ditemukan.");
            _logger = logger;
        }

        public async Task<JatuhTempoResultViewModel?> GetJatuhTempoAsync(string kontrakNo, DateTime perTanggal)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                // Query total angsuran jatuh tempo
                string sql = @"
                    SELECT
                        ja.kontrak_no,
                        k.client_name,
                        SUM(ja.angsuran_per_bulan) AS total_jatuh_tempo
                    FROM jadwal_angsuran ja
                    JOIN kontrak k ON k.kontrak_no = ja.kontrak_no
                    WHERE ja.kontrak_no = @kontrakNo
                      AND ja.tanggal_jatuh_tempo <= @perTanggal
                    GROUP BY ja.kontrak_no, k.client_name";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("kontrakNo", kontrakNo);
                cmd.Parameters.AddWithValue("perTanggal", perTanggal.Date);

                await using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync()) return null;

                var result = new JatuhTempoResultViewModel
                {
                    KontrakNo                = reader.GetString(0),
                    ClientName               = reader.GetString(1),
                    TotalAngsuranJatuhTempo  = reader.GetDecimal(2)
                };

                await reader.CloseAsync();

                // Query detail jadwal yang sudah jatuh tempo
                string sqlDetail = @"
                    SELECT angsuran_ke, angsuran_per_bulan, tanggal_jatuh_tempo
                    FROM jadwal_angsuran
                    WHERE kontrak_no = @kontrakNo
                      AND tanggal_jatuh_tempo <= @perTanggal
                    ORDER BY angsuran_ke";

                await using var cmdDetail = new NpgsqlCommand(sqlDetail, conn);
                cmdDetail.Parameters.AddWithValue("kontrakNo", kontrakNo);
                cmdDetail.Parameters.AddWithValue("perTanggal", perTanggal.Date);

                await using var readerDetail = await cmdDetail.ExecuteReaderAsync();
                while (await readerDetail.ReadAsync())
                {
                    result.DetailJadwal.Add(new JadwalAngsuran
                    {
                        KontrakNo         = kontrakNo,
                        AngsuranKe        = readerDetail.GetInt32(0),
                        AngsuranPerBulan  = readerDetail.GetDecimal(1),
                        TanggalJatuhTempo = readerDetail.GetDateTime(2)
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetJatuhTempo DB: {KontrakNo}", kontrakNo);
                throw;
            }
        }

        public async Task<List<DendaItemViewModel>> GetDendaAsync(string kontrakNo, DateTime perTanggal, int sudahBayarKe)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string sql = @"
                    WITH batas AS (
                        SELECT @perTanggal::date AS per_tanggal
                    ),
                    sudah_bayar AS (
                        SELECT @kontrakNo AS kontrak_no, @sudahBayarKe AS bayar_sampai_ke
                    )
                    SELECT
                        ja.kontrak_no,
                        k.client_name,
                        ja.angsuran_ke,
                        ja.angsuran_per_bulan,
                        (b.per_tanggal - ja.tanggal_jatuh_tempo) AS hari_keterlambatan,
                        ROUND(ja.angsuran_per_bulan * 0.001 * (b.per_tanggal - ja.tanggal_jatuh_tempo), 0) AS total_denda
                    FROM jadwal_angsuran ja
                    JOIN kontrak k      ON k.kontrak_no = ja.kontrak_no
                    JOIN sudah_bayar sb ON sb.kontrak_no = ja.kontrak_no
                    CROSS JOIN batas b
                    WHERE ja.angsuran_ke > sb.bayar_sampai_ke
                      AND ja.tanggal_jatuh_tempo <= b.per_tanggal
                    ORDER BY ja.angsuran_ke";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("kontrakNo",   kontrakNo);
                cmd.Parameters.AddWithValue("perTanggal",  perTanggal.Date);
                cmd.Parameters.AddWithValue("sudahBayarKe", sudahBayarKe);

                await using var reader = await cmd.ExecuteReaderAsync();
                var result = new List<DendaItemViewModel>();

                while (await reader.ReadAsync())
                {
                    result.Add(new DendaItemViewModel
                    {
                        KontrakNo           = reader.GetString(0),
                        ClientName          = reader.GetString(1),
                        AngsuranKe          = reader.GetInt32(2),
                        AngsuranPerBulan    = reader.GetDecimal(3),
                        HariKeterlambatan   = reader.GetInt32(4),
                        TotalDenda          = reader.GetDecimal(5)
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetDenda DB: {KontrakNo}", kontrakNo);
                throw;
            }
        }
    }
}
