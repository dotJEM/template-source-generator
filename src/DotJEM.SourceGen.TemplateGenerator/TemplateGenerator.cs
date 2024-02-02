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
        if (!Debugger.IsAttached)
            Debugger.Launch();

        IncrementalValueProvider<TemplateOptions> globalOptions = context.AnalyzerConfigOptionsProvider.Select((provider, token) =>
        {
            provider.GlobalOptions.TryGetValue($"build_property.RootNamespace", out string defaultNamespace);
            provider.GlobalOptions.TryGetValue($"build_property.DotJEMTemplateVisibility", out string defaultVisibility);
            return new TemplateOptions(defaultNamespace, defaultVisibility, null);
        });

        //var globalOptions = context.AnalyzerConfigOptionsProvider.Select(static (provider, _) =>
        //(
        //    Namespace: provider.GetGlobalOptionOrDefault("ConstEmbedNamespace", "Podimo.ConstEmbed"),
        //    Visibility: provider.GetGlobalOptionOrDefault("ConstEmbedVisibility", "internal")
        //));
        
        //IncrementalValueProvider<string> assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);
        //IncrementalValuesProvider<AdditionalText> templateFiles = context.AdditionalTextsProvider
        //    .Where(static file => file.Path.EndsWith(".sql")); //Note hardcoded filetype for now.

        //IncrementalValuesProvider<(AdditionalText Left, string Right)> combined = templateFiles
        //    .Combine(assemblyName);

        IncrementalValuesProvider<(AdditionalText text, TemplateOptions options)> templateFilesAndSettings = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (tuple, token) =>
            {
                (AdditionalText text, AnalyzerConfigOptionsProvider provider) = tuple;
                AnalyzerConfigOptions options = provider.GetOptions(text);
                options.TryGetValue($"build_metadata.AdditionalFiles.TemplateClass", out string className);
                options.TryGetValue($"build_metadata.AdditionalFiles.Visibility", out string visibility);
                options.TryGetValue($"build_metadata.AdditionalFiles.Namespace", out string @namespace);
                return (text, new TemplateOptions(@namespace, 
                    visibility ?? "internal", 
                    className ?? PascalCaseTranform.Transform(Path.GetExtension(text.Path).Trim('.'))));
                //if (!options.TryGetValue($"build_metadata.AdditionalFiles.ClassName", out string className))
                //    className = GenerateClassName(Path.GetExtension(text.Path).Trim('.'));
            });

        var merged = templateFilesAndSettings
            .Combine(globalOptions)
            .Select((tuple, token) =>
            {
                (AdditionalText text, TemplateOptions options) = tuple.Left;
                TemplateOptions defaults = tuple.Right;
                //options = options.ClassName == null
                //    ? options with { ClassName = GenerateClassName(Path.GetExtension(text.Path).Trim('.')) }
                //    : options;
                return (text, options: options | defaults);
            });


        //.Combine(globalOptions)
        //.Select((tuple, token) =>
        //{
        //    (AdditionalText text, TemplateOptions options) = tuple;
        //    options = options.ClassName == null
        //        ? options with { ClassName = GenerateClassName(Path.GetExtension(text.Path).Trim('.')) }
        //        : options;
        //    return (text, options);
        //});

        IncrementalValuesProvider<StringTemplate> templates = merged
            .SelectMany((tuple, token) => builder.Build(tuple.text, tuple.options, token));

        //context.RegisterPostInitializationOutput(ctx => );

        context.RegisterSourceOutput(templates, (spc, template) =>
        {
            spc.AddSource($"{template.Options.ClassName}.{template.Name}.{template.Key}.g.cs", template.ToString());
        });
    }
}