using System;
using System.Threading.Tasks;

namespace RecurrentPayment.Application.Services
{
    public interface IDailyEntryService
    {
        Task<Guid> SendDebit(int clientId, decimal value);
    }
}
