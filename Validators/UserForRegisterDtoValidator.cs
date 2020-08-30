using System;
using DatingApp.API.Dtos;
using FluentValidation;

namespace DatingApp.API.Validators
{
    public class UserForRegisterDtoValidator : AbstractValidator<UserForRegisterDto>
    {
        public UserForRegisterDtoValidator()
        {
            RuleFor(user => user.Username).NotEmpty();
            RuleFor(user => user.Password).NotEmpty().MinimumLength(4);
            RuleFor(user => user.Gender).NotEmpty();
            RuleFor(user => user.KnownAs).NotEmpty();
            RuleFor(user => user.DateOfBirth).NotEmpty();
            When(user => user.DateOfBirth != null, () =>
            {
                RuleFor(user => user.DateOfBirth).Must(dateOfBirth => IsValidRegistrationDate(dateOfBirth)).WithMessage("Must be 18 years old or above to register.");
            });
            RuleFor(user => user.City).NotEmpty();
            RuleFor(user => user.Country).NotEmpty();
        }
        public bool IsValidRegistrationDate(DateTime dateOfBirth)
        {
            if (DateTime.Now.Year - dateOfBirth.Year >= 18)
            {
                return true;
            }
            return false;
        }
    }

}