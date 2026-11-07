using FluentValidation;
using RailCat.Catalog.Api.Models.DTOs;

namespace RailCat.Catalog.Api.Validators;

public class CreateMenuItemValidator : AbstractValidator<CreateMenuItemDto>
{
    public CreateMenuItemValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CategoryId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ItemName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
    }
}

public class EmployeeVerificationValidator : AbstractValidator<EmployeeVerificationRequestDto>
{
    public EmployeeVerificationValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Employee ID is required.");
        RuleFor(x => x.DeviceToken).NotEmpty().WithMessage("Device token fingerprint is required.");
    }
}
