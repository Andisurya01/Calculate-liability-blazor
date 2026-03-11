using Npgsql;

namespace IMSFinance.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseHelper> _logger;

        public DatabaseHelper(IConfiguration config, ILogger<DatabaseHelper> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string tidak ditemukan.");
            _logger = logger;
        }

        public async Task SimpanKontrakAsync(string kontrakNo, string clientName, decimal otr)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string sql = @"
                    INSERT INTO kontrak (kontrak_no, client_name, otr)
                    VALUES (@kontrakNo, @clientName, @otr)
                    ON CONFLICT (kontrak_no) DO NOTHING";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("kontrakNo",  kontrakNo);
                cmd.Parameters.AddWithValue("clientName", clientName);
                cmd.Parameters.AddWithValue("otr",        otr);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal simpan kontrak: {KontrakNo}", kontrakNo);
                throw;
            }
        }

        public async Task SimpanJadwalAsync(List<(string kontrakNo, int ke, decimal nominal, DateTime tgl)> jadwal)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                foreach (var item in jadwal)
                {
                    string sql = @"
                        INSERT INTO jadwal_angsuran (kontrak_no, angsuran_ke, angsuran_per_bulan, tanggal_jatuh_tempo)
                        VALUES (@kontrakNo, @angsuranKe, @angsuranPerBulan, @tanggalJatuhTempo)
                        ON CONFLICT DO NOTHING";

                    await using var cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("kontrakNo",         item.kontrakNo);
                    cmd.Parameters.AddWithValue("angsuranKe",        item.ke);
                    cmd.Parameters.AddWithValue("angsuranPerBulan",  item.nominal);
                    cmd.Parameters.AddWithValue("tanggalJatuhTempo", item.tgl);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal simpan jadwal angsuran");
                throw;
            }
        }
    }
}
