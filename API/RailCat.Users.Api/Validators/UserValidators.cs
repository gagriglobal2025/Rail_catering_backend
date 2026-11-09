using FluentValidation;
using RailCat.Users.Api.Models.DTOs.Auth;
using RailCat.Users.Api.Models.DTOs.User;

namespace RailCat.Users.Api.Validators;

public class LoginValidator : AbstractValidator<LoginRequestDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.UserLogin)
            .NotEmpty()
            .WithMessage("User login (Email, Employee ID, or Mobile Number) is required.");

        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}

public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required.");
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).WithMessage("New password must be at least 6 characters.");
        RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword).WithMessage("New password and confirmation must match.");
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

public class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100).WithMessage("Full name is required.");
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
