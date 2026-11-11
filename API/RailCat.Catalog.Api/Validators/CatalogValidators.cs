using FluentValidation;
using RailCat.Catalog.Api.Models.DTOs;

namespace RailCat.Catalog.Api.Validators;

public class CreateMenuItemValidator : AbstractValidator<CreateMenuItemDto>
{
    public CreateMenuItemValidator()
    {
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

public class UpdateMenuItemValidator : AbstractValidator<UpdateMenuItemDto>
{
    public UpdateMenuItemValidator()
    {
        RuleFor(x => x.CategoryId).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.CategoryId));
        RuleFor(x => x.ItemName).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.ItemName));
        RuleFor(x => x.BasePrice!.Value).GreaterThanOrEqualTo(0).When(x => x.BasePrice.HasValue);
    }
}

public class UpdateConcessionPolicyValidator : AbstractValidator<UpdateConcessionPolicyDto>
{
    public UpdateConcessionPolicyValidator()
    {
        RuleFor(x => x.DiscountPercentage!.Value).InclusiveBetween(0, 100).When(x => x.DiscountPercentage.HasValue).WithMessage("Discount percentage must be between 0 and 100.");
    }
}

public class EmployeeStatusUpdateValidator : AbstractValidator<EmployeeStatusUpdateDto>
{
    private static readonly string[] AllowedStatuses = ["ACTIVE", "ACCEPTED", "APPROVED", "REJECTED", "PENDING"];

    public EmployeeStatusUpdateValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => !string.IsNullOrWhiteSpace(s) && AllowedStatuses.Contains(s.Trim().ToUpperInvariant()))
            .WithMessage("Status must be ACTIVE (or ACCEPTED), REJECTED, or PENDING.");
    }
}

