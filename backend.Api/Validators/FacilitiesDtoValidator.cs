namespace backend.Api.Validators;

using backend.Api.DTOs;
using FluentValidation;

public class FacilityDtoValidator : AbstractValidator<FacilityDto>
{
    //public FacilityDtoValidator()
    //{
    //    RuleFor(x => x.Name)
    //        .NotEmpty().WithMessage("Name is required.")
    //        .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

    //    RuleFor(x => x.Address)
    //        .NotNull().WithMessage("Address is required.");
    //}
}

