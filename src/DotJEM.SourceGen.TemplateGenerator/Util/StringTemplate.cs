namespace DotJEM.SourceGen.TemplateGenerator.Util;

public readonly record struct StringTemplate(
    string Namespace, string ClassName, string Visibility,
    string MethodName, string Source, string[] Args)
{
    public override string ToString()
    {
        return $$""""
                 namespace {{Namespace}};

                 {{Visibility}} static partial class {{ClassName}}
                 {
                      public static string {{MethodName}}({{string.Join(", ", Args)}})
                      {
                          return {{Source}};
                      }
                 }
                 """";
    }
}