﻿using LaptopStore.Application.Interfaces.Serialization.Serializers;
using FluentValidation;
using FluentValidation.Validators;

namespace LaptopStore.Application.Validators.Extensions
{
    public static class ValidatorExtensions
    {
        public static IRuleBuilderOptions<T, string> MustBeJson<T>(this IRuleBuilder<T, string> ruleBuilder, IPropertyValidator<T, string> validator) where T : class
        {
            return ruleBuilder.SetValidator(validator);
        }
    }
}