using FluentValidation;

namespace Pana.Api.Application.Production;

public class CreateRecipeRequestValidator : AbstractValidator<CreateRecipeRequest>
{
    public CreateRecipeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Recipe name is required.")
            .MaximumLength(300);

        RuleFor(x => x.Yield)
            .GreaterThan(0).WithMessage("Yield must be positive.");

        RuleFor(x => x.YieldUnit)
            .NotEmpty().WithMessage("Yield unit is required.")
            .MaximumLength(50);

        RuleFor(x => x.LaborCostPerUnit)
            .GreaterThanOrEqualTo(0).WithMessage("Labor cost cannot be negative.");

        RuleFor(x => x.EnergyCost)
            .GreaterThanOrEqualTo(0).WithMessage("Energy cost cannot be negative.");

        RuleFor(x => x.OverheadPct)
            .InclusiveBetween(0, 100).WithMessage("Overhead percentage must be between 0 and 100.");

        RuleForEach(x => x.Ingredients).SetValidator(new CreateRecipeIngredientRequestValidator());
    }
}

public class CreateRecipeIngredientRequestValidator : AbstractValidator<CreateRecipeIngredientRequest>
{
    public CreateRecipeIngredientRequestValidator()
    {
        RuleFor(x => x.MaterialId)
            .NotEmpty().WithMessage("Material ID is required.");

        RuleFor(x => x.Qty)
            .GreaterThan(0).WithMessage("Ingredient quantity must be positive.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(50);
    }
}
