namespace DotJEM.SourceGen.TemplateGenerator.Demo;

internal static partial class HtmlTemplates
{
     public static string HTMLPageOneTemplates_default(string title, string header, string body)
     {
         return "<!DOCTYPE html>\n\n<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">\n<head>\n    <meta charset=\"utf-8\" />\n    <title>" + title + "</title>\n</head>\n<body>\n    <h1>" + header + "</h1>\n    <section>" + body + "</section>\n</body>\n</html>\n";
     }
}