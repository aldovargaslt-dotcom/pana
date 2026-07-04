using FluentValidation;

namespace Pana.Api.Application.Production;

public class RawMaterialRequestValidator : AbstractValidator<RawMaterialRequest>
{
    public RawMaterialRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Material name is required.")
            .MaximumLength(300);

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(100);

        RuleFor(x => x.PurchaseUnit)
            .NotEmpty().WithMessage("Purchase unit is required.")
            .MaximumLength(50);

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase price cannot be negative.");

        RuleFor(x => x.PresentationQty)
            .GreaterThan(0).WithMessage("Presentation quantity must be positive.");

        RuleFor(x => x.BaseUnit)
            .NotEmpty().WithMessage("Base unit is required.")
            .MaximumLength(50);

        RuleFor(x => x.YieldPct)
            .InclusiveBetween(0.01m, 100).WithMessage("Yield percentage must be between 0.01 and 100.");

        RuleFor(x => x.Supplier)
            .MaximumLength(300);
    }
}
