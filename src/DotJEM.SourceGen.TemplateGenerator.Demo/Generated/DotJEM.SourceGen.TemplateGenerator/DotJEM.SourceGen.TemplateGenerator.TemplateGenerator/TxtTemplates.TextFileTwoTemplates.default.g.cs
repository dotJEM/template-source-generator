namespace DotJEM.SourceGen.TemplateGenerator.Demo;

internal static partial class TxtTemplates
{
     public static string TextFileTwoTemplates_default(string name, string request)
     {
         return "Hello " + name + ", give me your " + request + "\n";
     }
}