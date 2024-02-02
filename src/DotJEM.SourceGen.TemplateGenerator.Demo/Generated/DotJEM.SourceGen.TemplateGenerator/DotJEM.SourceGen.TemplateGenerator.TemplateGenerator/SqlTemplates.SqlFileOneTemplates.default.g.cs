namespace DotJEM.SourceGen.TemplateGenerator.Demo;

internal static partial class SqlTemplates
{
     public static string SqlFileOneTemplates_default(string table)
     {
         return "SELECT ONE FROM [" + table + "];\n";
     }
}