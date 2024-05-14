namespace DotJEM.SourceGen.TemplateGenerator.Util;

public readonly record struct StringTemplate(TemplateOptions Options, string MethodName, string Source, string[] Args)
{
    public override string ToString()
    {
        return $$""""
                 namespace {{Options.Namespace}};

                 {{Options.Visibility}} static partial class {{Options.ClassName}}
                 {
                      public static string {{MethodName}}({{string.Join(", ", Args)}})
                      {
                          return {{Source}};
                      }
                 }
                 """";
    }
}