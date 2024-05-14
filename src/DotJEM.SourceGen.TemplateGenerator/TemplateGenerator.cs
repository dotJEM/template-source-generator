using System;
using System.Diagnostics;
using System.IO;
using DotJEM.SourceGen.TemplateGenerator.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotJEM.SourceGen.TemplateGenerator;

/// <summary>
/// Document My Framework Class.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class TemplateGenerator : IIncrementalGenerator
{
    // SEE: https://github.com/podimo/Podimo.ConstEmbed/blob/develop/src/Podimo.ConstEmbed/ConstEmbedGenerator.cs
    // SEE: https://stackoverflow.com/questions/72095200/c-sharp-incremental-generator-how-i-can-read-additional-files-additionaltexts
    // https://andrewlock.net/creating-a-source-generator-part-6-saving-source-generator-output-in-source-control/
    // https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md

    private readonly StringTemplateBuilder builder = new StringTemplateBuilder();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (!Debugger.IsAttached)
        //    Debugger.Launch();

        IncrementalValueProvider<TemplateOptions> globalOptions = context.AnalyzerConfigOptionsProvider.Select((provider, token) =>
        {
            provider.GlobalOptions.TryGetValue($"build_property.RootNamespace", out string rootNamespace);
            provider.GlobalOptions.TryGetValue($"build_property.DotJEMTemplateVisibility", out string defaultVisibility);
            provider.GlobalOptions.TryGetValue($"build_property.DotJEMTemplateNamespace", out string defaultNamespace);
            provider.GlobalOptions.TryGetValue($"build_property.DotJEMTemplateMethodName", out string defaultMethodNameTemplate);
            provider.GlobalOptions.TryGetValue($"build_property.DotJEMTemplateParameterPattern", out string defaultParameterPattern);
            provider.GlobalOptions.TryGetValue($"build_property.DotJEMIncludeAllAdditionalFiles", out string includeAllAdditionalFiles);
            if(string.IsNullOrWhiteSpace(defaultNamespace)) defaultNamespace = rootNamespace;
            return new TemplateOptions(
                string.IsNullOrWhiteSpace(includeAllAdditionalFiles) || includeAllAdditionalFiles.Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase),
                defaultNamespace, defaultVisibility, null, defaultMethodNameTemplate, defaultParameterPattern);
        });

        IncrementalValuesProvider<(AdditionalText text, TemplateOptions options)> templateFilesAndSettings = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (tuple, token) =>
            {
                (AdditionalText text, AnalyzerConfigOptionsProvider provider) = tuple;
                AnalyzerConfigOptions options = provider.GetOptions(text);
                options.TryGetValue($"build_metadata.AdditionalFiles.TemplateClass", out string className);
                options.TryGetValue($"build_metadata.AdditionalFiles.Visibility", out string visibility);
                options.TryGetValue($"build_metadata.AdditionalFiles.Namespace", out string @namespace);
                options.TryGetValue($"build_metadata.AdditionalFiles.MethodNameTemplate", out string methodNameTemplate);
                options.TryGetValue($"build_metadata.AdditionalFiles.ParameterPattern", out string parameterPattern);
                options.TryGetValue($"build_metadata.AdditionalFiles.IsTemplates", out string isTemplates);
                return (text, 
                    new TemplateOptions(
                        string.IsNullOrWhiteSpace(isTemplates) || isTemplates.Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase),
                        @namespace, 
                        visibility ?? "internal", 
                        className ?? PascalCaseTranform.Transform(Path.GetExtension(text.Path).Trim('.')) + "Templates",
                        methodNameTemplate,
                        parameterPattern)
                    );
            });

        IncrementalValuesProvider<(AdditionalText text, TemplateOptions options)> merged = templateFilesAndSettings
            .Combine(globalOptions)
            .Select((tuple, token) =>
            {
                (AdditionalText text, TemplateOptions options) = tuple.Left;
                TemplateOptions defaults = tuple.Right;
                return (text, options: options | defaults);
            });

        IncrementalValuesProvider<StringTemplate> templates = merged
            .Where(tuple => tuple.options.ProcessAsTemplate is null or true)
            .SelectMany((tuple, token) => builder.Build(tuple.text, tuple.options, token));

        context.RegisterSourceOutput(templates, (spc, template) => {
            spc.AddSource($"{template.Options.ClassName}.{template.MethodName}.g.cs", template.ToString());
        });
    }
}