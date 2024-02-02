namespace DotJEM.SourceGen.TemplateGenerator.Util;

public readonly record struct TemplateOptions(string Namespace, string Visibility, string ClassName)
{
    public static TemplateOptions operator |(TemplateOptions left, TemplateOptions right)
        => new(left.Namespace ?? right.Namespace, left.Visibility ?? right.Visibility, left.ClassName ?? right.ClassName);
}