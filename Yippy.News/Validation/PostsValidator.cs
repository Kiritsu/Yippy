using FluentValidation;
using Yippy.Common.News;

namespace Yippy.News.Validation;

public class PostCreateRequestValidator : AbstractValidator<PostCreateRequest>
{
    public PostCreateRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(128);
        
        RuleFor(x => x.Body)
            .NotEmpty()
            .MaximumLength(65536);
    }
}