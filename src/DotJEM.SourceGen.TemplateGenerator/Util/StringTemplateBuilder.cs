using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace DotJEM.SourceGen.TemplateGenerator.Util;

public class StringTemplateBuilder
{
    private readonly Regex pattern;
    private readonly Regex nlPattern = new Regex(@"\r\n?|\n", RegexOptions.Compiled);

    public StringTemplateBuilder(string pattern = "@\\{(.+?)}", RegexOptions options = RegexOptions.Compiled)
        : this(new Regex(pattern, options))
    {
    }

    public StringTemplateBuilder(Regex pattern)
    {
        this.pattern = pattern;
    }

    public IEnumerable<StringTemplate> Build(AdditionalText content, TemplateOptions options, CancellationToken token)
    {
        string name = PascalCaseTranform.Transform(Path.GetFileNameWithoutExtension(content.Path));
        string sourceFromFile = content.GetText(token)!.ToString();

        Regex parameterPattern = options.ParameterPatternRegex ?? pattern;
        foreach ((string Key, string Template) in new TemplateReader().ReadToEnd(new StringReader(sourceFromFile)))
        {
            string source = Template;
            int index = 0;
            StringBuilder builder = new StringBuilder();
            HashSet<string> args = new();

            source = nlPattern.Replace(source, "\\n");
            foreach (Match match in parameterPattern.Matches(source).Cast<Match>())
            {
                string before = source.Substring(index, match.Index - index).Replace("\"", "\\\"");
                string key = match.Groups[1].Value;
                args.Add($"string {key}");
                if (index > 0)
                    builder.Append(" + ");

                builder.Append("\"");
                builder.Append(before);
                builder.Append("\" + ");
                builder.Append(key);

                index = match.Index + key.Length + 3;
            }
            string remainder = source.Substring(index).Replace("\"", "\\\"");
            if (index > 0)
                builder.Append(" + ");
            builder.Append("\"");
            builder.Append(remainder);
            builder.Append("\"");

            string methodName = GenerateMethodName(options, name, PascalCaseTranform.Transform(Key));
            yield return new(options, methodName, builder.ToString(), args.ToArray());
        }
    }

    public string GenerateMethodName(TemplateOptions options, string templateName, string templateKey)
    {
        if (string.IsNullOrWhiteSpace(options.MethodNameTemplate))
            return string.IsNullOrWhiteSpace(templateKey) ? templateName : $"{templateName}_{templateKey}";

        Regex parameterPattern = options.ParameterPatternRegex ?? pattern;
        string[] templates = options.MethodNameTemplate.Split('|');

        string multiTemplatesTemplate = templates[0];
        string singleTemplatesTemplate= templates[0];
        if (templates.Length >1)
            singleTemplatesTemplate= templates[1];

        string templateToUse = string.IsNullOrWhiteSpace(templateKey) 
            ? singleTemplatesTemplate 
            : multiTemplatesTemplate;

        return parameterPattern.Replace(templateToUse, match =>
        {
            string key = match.Groups[1].Value;
            return key switch
            {
                "name" => templateName,
                "templateName" => templateName,
                "key" => templateKey,
                "templateKey" => templateKey,
                _ => match.Value
            };
        });
    }
}

