using backend.Api.DTOs;
using FluentValidation;

namespace backend.Api.Validators;

public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.StreetAddress)
            .NotEmpty().WithMessage("Street is required");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required");

        
    }
}

