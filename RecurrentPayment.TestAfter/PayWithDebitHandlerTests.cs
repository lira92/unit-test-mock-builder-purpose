using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using RecurrentPayment.Application.Handlers;
using RecurrentPayment.Application.Repositories;
using RecurrentPayment.Application.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RecurrentPayment.TestAfter
{
    [Trait("Unit", nameof(PayWithDebitHandler))]
    public class PayWithDebitHandlerTests
    {
        [Fact]
        public async Task IfRequestIsInvalid_ShouldReturnErrors()
        {
            var handler = new HandlerBuilder()
                .WithRequestInvalid()
                .Build();

            var errors = await handler.Handle(new PayWithDebitRequest());

            errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task IfClientDoesNotHaveSufficienteBalance_ShouldReturnErrors()
        {
            var request = new PayWithDebitRequest { ClientId = 10, Value = 5 };
            var handler = new HandlerBuilder()
                .WithRequestValid()
                .WithInsufficientBalance(request.ClientId, request.Value)
                .Build();

            var errors = await handler.Handle(request);

            errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task IfDailyEntryServiceFails_ShouldReturnErrors()
        {
            var request = new PayWithDebitRequest { ClientId = 10, Value = 5 };
            var handler = new HandlerBuilder()
                .WithRequestValid()
                .WithSufficientBalance(request.ClientId, request.Value)
                .WithDailyEntryServiceFailing(request.ClientId, request.Value)
                .Build();

            var errors = await handler.Handle(request);

            errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task IfRecurrentPaymentWasSaveWithSuccessfully_ShouldNotReturnErrors()
        {
            var request = new PayWithDebitRequest { ClientId = 10, Value = 5 };
            var (handler, mockAssertions) = new HandlerBuilder()
                .WithRequestValid()
                .WithSufficientBalance(request.ClientId, request.Value)
                .BuildWithMock();

            var errors = await handler.Handle(request);

            errors.Should().BeEmpty();
            mockAssertions
                .ShouldCallRepository();
        }

        private class HandlerBuilder
        {
            private readonly Mock<IValidator<PayWithDebitRequest>> _mockValidator;
            private readonly Mock<ICheckingAccountBalanceService> _mockCheckingAccountBalanceService;
            private readonly Mock<IDailyEntryService> _mockDailyEntryService;
            private readonly Mock<IRecurrentPaymentRepository> _mockRecurrentPaymentRepository;

            public HandlerBuilder()
            {
                _mockValidator = new Mock<IValidator<PayWithDebitRequest>>();
                _mockCheckingAccountBalanceService = new Mock<ICheckingAccountBalanceService>();
                _mockDailyEntryService = new Mock<IDailyEntryService>();
                _mockRecurrentPaymentRepository = new Mock<IRecurrentPaymentRepository>();
            }

            public HandlerBuilder WithRequestInvalid()
            {
                var validationResult = new ValidationResult(
                    new List<ValidationFailure>() {
                        new ValidationFailure("someProperty", "someError")
                    }
                );
                _mockValidator.Setup(x => x.Validate(It.IsAny<PayWithDebitRequest>()))
                    .Returns(validationResult);

                return this;
            }

            public HandlerBuilder WithRequestValid()
            {
                var validationResult = new ValidationResult();
                _mockValidator.Setup(x => x.Validate(It.IsAny<PayWithDebitRequest>()))
                    .Returns(validationResult);

                return this;
            }

            public HandlerBuilder WithInsufficientBalance(int clientId, decimal valueRequested)
            {
                _mockCheckingAccountBalanceService
                    .Setup(x => x.HasSufficientBalance(clientId, valueRequested))
                    .ReturnsAsync(false);

                return this;
            }

            public HandlerBuilder WithSufficientBalance(int clientId, decimal valueRequested)
            {
                _mockCheckingAccountBalanceService
                    .Setup(x => x.HasSufficientBalance(clientId, valueRequested))
                    .ReturnsAsync(true);

                return this;
            }

            public HandlerBuilder WithDailyEntryServiceFailing(int clientId, decimal valueRequested)
            {
                _mockDailyEntryService
                    .Setup(x => x.SendDebit(clientId, valueRequested))
                    .Throws(new Exception("Unexpected error"));

                return this;
            }

            public PayWithDebitHandler Build()
            {
                return new PayWithDebitHandler(
                    _mockValidator.Object,
                    _mockCheckingAccountBalanceService.Object,
                    _mockDailyEntryService.Object,
                    _mockRecurrentPaymentRepository.Object
                );
            }

            public (PayWithDebitHandler, MockAssertions) BuildWithMock()
            {
                return (Build(), new MockAssertions(_mockRecurrentPaymentRepository));
            }
        }

        private class MockAssertions
        {
            private readonly Mock<IRecurrentPaymentRepository> _mockRecurrentPaymentRepository;

            public MockAssertions(Mock<IRecurrentPaymentRepository> mockRecurrentPaymentRepository)
            {
                _mockRecurrentPaymentRepository = mockRecurrentPaymentRepository;
            }

            public MockAssertions ShouldCallRepository()
            {
                _mockRecurrentPaymentRepository
                    .Verify(x => x.CreateAsync(
                        It.IsAny<Application.Entities.RecurrentPayment>()),
                        Times.Once()
                     );

                return this;
            }
        }
    }
}
