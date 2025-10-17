using FluentValidation;
using laboratorul3.Features.Books.Commands;

namespace laboratorul3.Features.Books.Validators
{
    public class DeleteBookCommandValidator : AbstractValidator<DeleteBookCommand>
    {
        public DeleteBookCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Book ID must be greater than 0");
        }
    }
}