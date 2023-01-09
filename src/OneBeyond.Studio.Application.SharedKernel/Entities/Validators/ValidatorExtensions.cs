using System;
using System.Text;
using EnsureThat;
using FluentValidation;
using OneBeyond.Studio.Crosscuts.Exceptions;

namespace OneBeyond.Studio.Application.SharedKernel.Entities.Validators;

/// <summary>
/// Validation extensions
/// </summary>
public static class ValidatorExtensions
{
    /// <summary>
    /// Ensure entity is valid against the validator
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="validator"></param>
    /// <param name="entity"></param>
    public static void EnsureIsValid<TEntity>(this IValidator<TEntity> validator, TEntity entity)
        where TEntity : class
        => validator.EnsureIsValid(entity, (message) => new Exceptions.ValidationException(message));

    /// <summary>
    /// Ensure entity is valid against the validator
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="validator"></param>
    /// <param name="entity"></param>
    /// <param name="exceptionCtor"></param>
    public static void EnsureIsValid<TEntity>(this IValidator<TEntity> validator, TEntity entity, Func<string, OneBeyondException> exceptionCtor)
        where TEntity : class
    {
        EnsureArg.IsNotNull(validator, nameof(validator));
        EnsureArg.IsNotNull(entity, nameof(entity));
        EnsureArg.IsNotNull(exceptionCtor, nameof(exceptionCtor));

        var validationResult = validator.Validate(entity);
        if (!validationResult.IsValid)
        {
            var errorMessages = new StringBuilder();
            validationResult.Errors
                .ForEach(
                    (failure) => errorMessages.AppendLine(failure.ErrorMessage));
            throw exceptionCtor(errorMessages.ToString());
        }
    }
}
