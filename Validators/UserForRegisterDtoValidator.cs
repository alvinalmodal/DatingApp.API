using DatingApp.API.Dtos;
using FluentValidation;

namespace DatingApp.API.Validators
{
    public class UserForRegisterDtoValidator : AbstractValidator<UserForRegisterDto>
    {
        public UserForRegisterDtoValidator()
        {
            RuleFor(user => user.Username).NotEmpty();
            RuleFor(user => user.Password).MinimumLength(4);
        }
    }
}