using FluentValidation;
using RailCat.Users.Api.Models.DTOs.Auth;
using RailCat.Users.Api.Models.DTOs.User;

namespace RailCat.Users.Api.Validators;

public class LoginValidator : AbstractValidator<LoginRequestDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.EmailOrEmployeeId).NotEmpty().WithMessage("Email or Employee ID is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}

public class CreateStaffUserValidator : AbstractValidator<CreateStaffUserDto>
{
    public CreateStaffUserValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty().MaximumLength(30);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(120);
        RuleFor(x => x.SystemRole).Must(r => r == "ADMIN" || r == "COUNTER_OPERATOR" || r == "KITCHEN_STAFF")
            .WithMessage("SystemRole must be ADMIN, COUNTER_OPERATOR, or KITCHEN_STAFF.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

