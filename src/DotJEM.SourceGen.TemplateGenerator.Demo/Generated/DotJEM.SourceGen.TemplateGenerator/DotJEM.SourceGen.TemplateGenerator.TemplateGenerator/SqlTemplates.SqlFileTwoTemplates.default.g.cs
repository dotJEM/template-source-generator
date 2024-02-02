namespace DotJEM.SourceGen.TemplateGenerator.Demo;

internal static partial class SqlTemplates
{
     public static string SqlFileTwoTemplates_default(string table)
     {
         return "SELECT ONE, TWO FROM [" + table + "];\n";
     }
}