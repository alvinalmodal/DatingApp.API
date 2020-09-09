using DatingApp.API.Dtos;
using FluentValidation;
using DatingApp.API.Data;

namespace DatingApp.API.Validators
{
    public class UserForLoginDtoValidator : AbstractValidator<UserForLoginDto>
    {
        public UserForLoginDtoValidator()
        {
            RuleFor(user => user.Username).NotEmpty();
            RuleFor(user => user.Password).MinimumLength(4);
        }
    }
}