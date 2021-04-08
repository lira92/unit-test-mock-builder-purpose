using System.Threading.Tasks;

namespace RecurrentPayment.Application.Repositories
{
    public interface IRecurrentPaymentRepository
    {
        Task CreateAsync(Entities.RecurrentPayment recurrentPayment);
    }
}