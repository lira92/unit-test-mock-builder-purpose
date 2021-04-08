using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using RecurrentPayment.Application.Handlers;
using RecurrentPayment.Application.Repositories;
using RecurrentPayment.Application.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RecurrentPayment.TestBefore
{
    [Trait("Unit", nameof(PayWithDebitHandler))]
    public class PayWithDebitHandlerTests
    {
        [Fact]
        public async Task IfRequestIsInvalid_ShouldReturnErrors()
        {
            var mockValidator = new Mock<IValidator<PayWithDebitRequest>>();
            var validationResult = new ValidationResult(
                new List<ValidationFailure>() {
                    new ValidationFailure("someProperty", "someError")
                }
            );
            mockValidator.Setup(x => x.Validate(It.IsAny<PayWithDebitRequest>()))
                .Returns(validationResult);
            var mockCheckBalanceService = new Mock<ICheckingAccountBalanceService>();
            var mockDailyEntryService = new Mock<IDailyEntryService>();
            var mockRepository = new Mock<IRecurrentPaymentRepository>();

            var handler = new PayWithDebitHandler(
                mockValidator.Object,
                mockCheckBalanceService.Object,
                mockDailyEntryService.Object,
                mockRepository.Object
            );

            var errors = await handler.Handle(new PayWithDebitRequest());

            errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task IfClientDoesNotHaveSufficienteBalance_ShouldReturnErrors()
        {
            var request = new PayWithDebitRequest { ClientId = 10, Value = 5 };
            var mockValidator = new Mock<IValidator<PayWithDebitRequest>>();
            var validationResult = new ValidationResult();
            mockValidator.Setup(x => x.Validate(It.IsAny<PayWithDebitRequest>()))
                .Returns(validationResult);
            var mockCheckBalanceService = new Mock<ICheckingAccountBalanceService>();
            mockCheckBalanceService
                .Setup(x => x.HasSufficientBalance(request.ClientId, request.Value))
                .ReturnsAsync(false);
            var mockDailyEntryService = new Mock<IDailyEntryService>();
            var mockRepository = new Mock<IRecurrentPaymentRepository>();

            var handler = new PayWithDebitHandler(
                mockValidator.Object,
                mockCheckBalanceService.Object,
                mockDailyEntryService.Object,
                mockRepository.Object
            );

            var errors = await handler.Handle(request);

            errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task IfDailyEntryServiceFails_ShouldReturnErrors()
        {
            var request = new PayWithDebitRequest { ClientId = 10, Value = 5 };
            var mockValidator = new Mock<IValidator<PayWithDebitRequest>>();
            var validationResult = new ValidationResult();
            mockValidator.Setup(x => x.Validate(It.IsAny<PayWithDebitRequest>()))
                .Returns(validationResult);
            var mockCheckBalanceService = new Mock<ICheckingAccountBalanceService>();
            mockCheckBalanceService
                .Setup(x => x.HasSufficientBalance(request.ClientId, request.Value))
                .ReturnsAsync(true);
            var mockDailyEntryService = new Mock<IDailyEntryService>();
            mockDailyEntryService
                .Setup(x => x.SendDebit(request.ClientId, request.Value))
                .Throws(new System.Exception("Unexpected error"));
            var mockRepository = new Mock<IRecurrentPaymentRepository>();

            var handler = new PayWithDebitHandler(
                mockValidator.Object,
                mockCheckBalanceService.Object,
                mockDailyEntryService.Object,
                mockRepository.Object
            );

            var errors = await handler.Handle(request);

            errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task IfRecurrentPaymentWasSaveWithSuccessfully_ShouldNotReturnErrors()
        {
            var request = new PayWithDebitRequest { ClientId = 10, Value = 5 };
            var mockValidator = new Mock<IValidator<PayWithDebitRequest>>();
            var validationResult = new ValidationResult();
            mockValidator.Setup(x => x.Validate(It.IsAny<PayWithDebitRequest>()))
                .Returns(validationResult);
            var mockCheckBalanceService = new Mock<ICheckingAccountBalanceService>();
            mockCheckBalanceService
                .Setup(x => x.HasSufficientBalance(request.ClientId, request.Value))
                .ReturnsAsync(true);
            var mockDailyEntryService = new Mock<IDailyEntryService>();
            var mockRepository = new Mock<IRecurrentPaymentRepository>();

            var handler = new PayWithDebitHandler(
                mockValidator.Object,
                mockCheckBalanceService.Object,
                mockDailyEntryService.Object,
                mockRepository.Object
            );

            var errors = await handler.Handle(request);

            errors.Should().BeEmpty();
        }
    }
}
