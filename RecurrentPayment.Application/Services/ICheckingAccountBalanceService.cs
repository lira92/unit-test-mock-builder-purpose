using System.Threading.Tasks;

namespace RecurrentPayment.Application.Services
{
    public interface ICheckingAccountBalanceService
    {
        Task<bool> HasSufficientBalance(int clientId, decimal valueRequested);
    }
}
