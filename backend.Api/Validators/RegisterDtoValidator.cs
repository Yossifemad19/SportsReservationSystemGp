using backend.Api.DTOs;
using FluentValidation;

namespace backend.Api.Validators;

public class RegisterDtoValidator:AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid Email Format");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name Is Required")
            .Length(2, 20).WithMessage("First Name Is Must Be Between 2 and 20 Characters");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name Is Required")
            .Length(2, 20).WithMessage("Last Name Is Must Be Between 2 and 20 Characters");
        
        RuleFor(x=>x.PhoneNumber).Length(11)
            .Matches(@"^0\d{10}$")
            .When(x => x.PhoneNumber is not null)
            .WithMessage("Invalid phone number format.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("Password must contain at least one number")
            .Matches(@"[\W]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmedPassword).NotEmpty().Equal(x => x.Password).WithMessage("Password not match");

    }
}


public class OwnerRegisterDtoValidator : AbstractValidator<OwnerRegisterDto>
{
    public OwnerRegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid Email Format");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name Is Required")
            .Length(2, 20).WithMessage("First Name Is Must Be Between 2 and 20 Characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name Is Required")
            .Length(2, 20).WithMessage("Last Name Is Must Be Between 2 and 20 Characters");

        RuleFor(x => x.PhoneNumber).Length(11)
            .Matches(@"^0\d{10}$")
            .When(x => x.PhoneNumber is not null)
            .WithMessage("Invalid phone number format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("Password must contain at least one number")
            .Matches(@"[\W]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword).NotEmpty().Equal(x => x.Password).WithMessage("Password not match");

    }
}