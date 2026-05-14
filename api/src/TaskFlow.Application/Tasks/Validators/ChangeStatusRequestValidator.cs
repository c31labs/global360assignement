using FluentValidation;
using TaskFlow.Application.Tasks.Dtos;

namespace TaskFlow.Application.Tasks.Validators;

public sealed class ChangeStatusRequestValidator : AbstractValidator<ChangeStatusRequest>
{
    public ChangeStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
