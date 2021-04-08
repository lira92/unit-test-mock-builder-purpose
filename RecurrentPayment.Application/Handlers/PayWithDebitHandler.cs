using FluentValidation;
using RecurrentPayment.Application.Repositories;
using RecurrentPayment.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecurrentPayment.Application.Handlers
{
    public class PayWithDebitHandler
    {
        private readonly IValidator<PayWithDebitRequest> _validator;
        private readonly ICheckingAccountBalanceService _checkingAccountBalanceService;
        private readonly IDailyEntryService _dailyEntryService;
        private readonly IRecurrentPaymentRepository _recurrentPaymentRepository;

        public PayWithDebitHandler(
            IValidator<PayWithDebitRequest> validator,
            ICheckingAccountBalanceService checkingAccountBalanceService,
            IDailyEntryService dailyEntryService,
            IRecurrentPaymentRepository recurrentPaymentRepository
        )
        {
            _validator = validator;
            _checkingAccountBalanceService = checkingAccountBalanceService;
            _dailyEntryService = dailyEntryService;
            _recurrentPaymentRepository = recurrentPaymentRepository;
        }

        public async Task<IEnumerable<string>> Handle(PayWithDebitRequest request)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return validationResult.Errors.Select(x => x.ErrorMessage);
            }

            if (!await _checkingAccountBalanceService.HasSufficientBalance(request.ClientId, request.Value))
            {
                return new List<string> { "Client does not have sufficiente balance." };
            }

            Guid transactionId = Guid.Empty;
            try
            {
                transactionId = await _dailyEntryService.SendDebit(request.ClientId, request.Value);
            }
            catch(Exception)
            {
                return new List<string> { "It was not possible to debit checking account." };
            }

            var recurrentPayment = new Entities.RecurrentPayment(request.ClientId, request.Value, transactionId);

            await _recurrentPaymentRepository.CreateAsync(recurrentPayment);

            return new List<string>();
        }
    }
}
