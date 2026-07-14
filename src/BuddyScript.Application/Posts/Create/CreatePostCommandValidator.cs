using FluentValidation;

namespace BuddyScript.Application.Posts.Create;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        // A post needs text OR an image; empty text is allowed only when an image is attached.
        RuleFor(x => x.Content)
            .NotEmpty()
            .When(x => x.ImageStream is null)
            .WithMessage("Post must contain text or an image.");

        RuleFor(x => x.Content).MaximumLength(5000);
        RuleFor(x => x.Visibility).IsInEnum();
    }
}
