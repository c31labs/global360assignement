using FluentValidation;
using TaskFlow.Application.Tasks.Dtos;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.Validators;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(TaskItem.MaxTitleLength);

        RuleFor(x => x.Description)
            .MaximumLength(TaskItem.MaxDescriptionLength)
            .When(x => x.Description is not null);

        RuleFor(x => x.Assignee)
            .MaximumLength(TaskItem.MaxAssigneeLength)
            .When(x => x.Assignee is not null);

        RuleFor(x => x.Priority).IsInEnum();
    }
}
