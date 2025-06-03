namespace backend.Api.Validators;

using backend.Api.DTOs;
using FluentValidation;

public class FacilityDtoValidator : AbstractValidator<FacilityDto>
{
    public FacilityDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2,20).WithMessage("Name  Must Be Between 2 and 20 Characters");

        RuleFor(x => x.Address)
            .NotNull().WithMessage("Address is required")
            .SetValidator(new AddressDtoValidator());
    }
}

