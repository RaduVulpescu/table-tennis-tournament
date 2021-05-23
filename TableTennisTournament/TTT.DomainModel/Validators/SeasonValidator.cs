using System;
using FluentValidation;
using TTT.DomainModel.DTO;

namespace TTT.DomainModel.Validators
{
    public class SeasonValidator : AbstractValidator<SeasonsPatchDTO>
    {
        public SeasonValidator()
        {
            RuleFor(x => x.EndDate).LessThanOrEqualTo(DateTime.Now);
        }
    }
}
