using FluentValidation;
using RailCat.Orders.Api.Models.DTOs;

namespace RailCat.Orders.Api.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequestDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerType).Must(t => t == "NORMAL" || t == "RAILWAY_EMPLOYEE")
            .WithMessage("CustomerType must be NORMAL or RAILWAY_EMPLOYEE.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("An order must contain at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

