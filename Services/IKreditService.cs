using IMSFinance.Models.ViewModels;

namespace IMSFinance.Services
{
    // Interface untuk data dari PostgreSQL
    public interface IKreditService
    {
        Task<JatuhTempoResultViewModel?> GetJatuhTempoAsync(string kontrakNo, DateTime perTanggal);
        Task<List<DendaItemViewModel>> GetDendaAsync(string kontrakNo, DateTime perTanggal, int sudahBayarKe);
    }

    // Interface untuk data dari JSON
    public interface IKreditJsonService
    {
        Task<JatuhTempoResultViewModel?> GetJatuhTempoAsync(string kontrakNo, DateTime perTanggal);
        Task<List<DendaItemViewModel>> GetDendaAsync(string kontrakNo, DateTime perTanggal, int sudahBayarKe);
        Task<List<string>> GetAllKontrakNoAsync();
    }
}
