using System;
using System.Linq;
using System.Reflection;
using Ardalis.SmartEnum;
using Humanizer;

namespace OneBeyond.Studio.Application.SharedKernel.Specifications;

/// <summary>
/// </summary>
public abstract class CasingPolicy : SmartEnum<CasingPolicy>
{
    /// <summary>
    /// Search for the propertyInfo after having changed the string: "propertyName" => "PropertyName"
    /// </summary>
    public static readonly CasingPolicy SentenceCase = new SentenceCasePolicy();

    /// <summary>
    /// Search for the propertyInfo after having changed the string: "PropertyName" => "propertyName"
    /// </summary>
    public static readonly CasingPolicy FirstLetterCase = new FirstLetterCasePolicy();

    /// <summary>
    /// Search for the propertyInfo after having changed the string: "PropertyName" => "propertyname";
    /// </summary>
    public static readonly CasingPolicy LowerCase = new LowerCasePolicy();

    /// <summary>
    /// Search for the propertyInfo in a greedy way, ignoring any case policy
    /// </summary>
    public static readonly CasingPolicy CaseInsensitive = new CaseInsensitivePolicy();


    private CasingPolicy(string name, int value) : base(name, value)
    {
    }

    /// <summary>
    /// </summary>
    public abstract PropertyInfo GetProperty(Type type, string propertyName);

    private sealed class SentenceCasePolicy : CasingPolicy
    {
        public SentenceCasePolicy()
            : base("SentenceCase", 1)
        {
        }

        public override PropertyInfo GetProperty(Type type, string propertyName)
        {
            var transformedPropName = propertyName.Transform(To.SentenceCase);
            return type.GetProperties()
                .FirstOrDefault(prop => prop.Name.Equals(transformedPropName))!;
        }
    }

    private sealed class FirstLetterCasePolicy : CasingPolicy
    {
        public FirstLetterCasePolicy()
            : base("FirstLetterCase", 2)
        {
        }

        public override PropertyInfo GetProperty(Type type, string propertyName)
        {
            var transformedPropName = propertyName.Transform(new FirstLetterLowerCase());
            return type.GetProperties()
                .FirstOrDefault(prop => prop.Name.Equals(transformedPropName))!;
        }

        private sealed class FirstLetterLowerCase : IStringTransformer
        {
            public string Transform(string input)
            {
                return !AllCapitals(input)
                    ? input.Length >= 1
                        ? string.Concat(input.Substring(0, 1).ToLower(), input.Substring(1))
                        : input.ToLower()
                    : input;
            }

            private static bool AllCapitals(string input)
                => input.ToUpper() == input;
        }
    }

    private sealed class LowerCasePolicy : CasingPolicy
    {
        public LowerCasePolicy()
            : base("LowerCase", 3)
        {
        }

        public override PropertyInfo GetProperty(Type type, string propertyName)
        {
            var transformedPropName = propertyName.Transform(To.LowerCase);
            return type.GetProperties()
                .FirstOrDefault(prop => prop.Name.Equals(transformedPropName))!;
        }
    }

    private sealed class CaseInsensitivePolicy : CasingPolicy
    {
        public CaseInsensitivePolicy()
            : base("CaseInsensitive", 4)
        {
        }

        public override PropertyInfo GetProperty(Type type, string propertyName)
        {
            return type.GetProperties()
                .FirstOrDefault(prop => prop.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))!;
        }
    }
}

/// <summary>
/// </summary>
public static class PoliciesExtensions
{
    /// <summary>
    /// </summary>
    public static PropertyInfo GetProperty(this CasingPolicy[] policies, Type type, string propertyName)
    {
        foreach (var policy in policies)
        {
            var propertyInfo = policy.GetProperty(type, propertyName);
            if (propertyInfo != null)
            {
                return propertyInfo;
            }
        }
        return null!;
    }
}
