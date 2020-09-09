using DatingApp.API.Dtos;
using FluentValidation;
using DatingApp.API.Data;

namespace DatingApp.API.Validators
{
    public class UserForUpdateDtoValidator : AbstractValidator<UserForUpdateDto>
    {
        public UserForUpdateDtoValidator()
        {
            RuleFor(user => user.Introduction).NotEmpty();
            RuleFor(user => user.LookingFor).NotEmpty();
            RuleFor(user => user.Interests).NotEmpty();
            RuleFor(user => user.City).NotEmpty();
            RuleFor(user => user.Country).NotEmpty();
        }
    }
}