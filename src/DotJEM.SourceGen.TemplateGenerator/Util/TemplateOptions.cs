using System.Text.RegularExpressions;

namespace DotJEM.SourceGen.TemplateGenerator.Util;

public readonly record struct TemplateOptions(bool? ProcessAsTemplate, string Namespace, string Visibility, string ClassName, string MethodNameTemplate, string ParameterPattern)
{
    public static TemplateOptions operator |(TemplateOptions left, TemplateOptions defaults)
        => new(
            left.ProcessAsTemplate ?? defaults.ProcessAsTemplate,
            Select(left.Namespace , defaults.Namespace),
            Select(left.Visibility, defaults.Visibility), 
            Select(left.ClassName, defaults.ClassName),
            Select(left.MethodNameTemplate, defaults.MethodNameTemplate),
            Select(left.ParameterPattern, defaults.ParameterPattern));

    private static string Select(string one, string other)
        => !string.IsNullOrWhiteSpace(one) ? one : other;

    public Regex ParameterPatternRegex { get; } = string.IsNullOrWhiteSpace(ParameterPattern) 
        ? null : new Regex(ParameterPattern, RegexOptions.Compiled);

}