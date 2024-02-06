using DotJEM.SourceGen.TemplateGenerator.Demo;
using NUnit.Framework;

namespace DotJEM.SourceGen.TemplateGenerator.Test;

public class MyFrameworkClassTest
{
    [Test]
    public void SayHello_ReturnsHello()
    {

        Assert.That(TextTemplates.TextFileThreeTemplates_default("f"), Is.EqualTo(""));
    }
}