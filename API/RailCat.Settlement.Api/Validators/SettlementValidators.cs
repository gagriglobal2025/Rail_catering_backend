using FluentValidation;
using RailCat.Settlement.Api.Models.DTOs;

namespace RailCat.Settlement.Api.Validators;

public class OpenShiftValidator : AbstractValidator<OpenShiftRequestDto>
{
    public OpenShiftValidator()
    {
        RuleFor(x => x.CashierUserId).NotEmpty().WithMessage("Cashier user ID is required.");
        RuleFor(x => x.StartingCash).GreaterThanOrEqualTo(0);
    }
}

public class ProcessPaymentValidator : AbstractValidator<ProcessPaymentRequestDto>
{
    public ProcessPaymentValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order ID is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.PaymentMethod).Must(m => m == "UPI" || m == "CARD" || m == "CASH")
            .WithMessage("PaymentMethod must be UPI, CARD, or CASH.");
    }
}

