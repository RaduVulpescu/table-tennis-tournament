using System;
using FluentValidation;
using TTT.DomainModel.DTO;

namespace TTT.DomainModel.Validators
{
    public class PlayerValidator : AbstractValidator<PlayerDTO>
    {
        public PlayerValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.BirthYear).InclusiveBetween(1900, DateTime.Now.Year);
            RuleFor(x => x.Height).InclusiveBetween(20, 250);
            RuleFor(x => x.Weight).InclusiveBetween(10, 200);
            RuleFor(x => x.CurrentLevel).IsInEnum();
        }
    }
}
