using FluentValidation;
using laboratorul3.Features.Books.Commands;

namespace laboratorul3.Features.Books.Validators
{
    public class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
    {
        public UpdateBookCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Book ID must be greater than 0");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author is required")
                .MaximumLength(100).WithMessage("Author must not exceed 100 characters");

            RuleFor(x => x.Year)
                .GreaterThan(0).WithMessage("Year must be greater than 0")
                .LessThanOrEqualTo(System.DateTime.Now.Year).WithMessage("Year cannot be in the future");
        }
    }
}