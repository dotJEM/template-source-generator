# Template Source Generater

This is a small C# Source Generater for converting various template files into C# code.

# Examples

Person.xml
```
<person>
  <name value="@{name}">
  <age value="@{age}">
</person>
```

Would generate a method with the following signature:
```
internal static class XmlTemplates {
    public static string Person(string name, string age) => ...;
}
```

Usage:
```
string result = XmlTemplates.Person("Peter", 42);

-->
<person>
  <name value="Peter">
  <age value="42">
</person>
```


# Options
The following global options can be used to control the generation process:

| Option                           | Default                 | Description                                                            |
| -------------------------------- | ----------------------- | ---------------------------------------------------------------------- |
| DotJEMTemplateVisibility         | internal                | The visibility to use for the templates class, e.g. internal or public |
| DotJEMTemplateNamespace          | <RootNamespace>         | The namespace to to put the templates class under, defaults to the configured RootNamespace |
| DotJEMTemplateMethodName         | @{name}_@{key}\|@{name} | A Template string to use when giving themplates method name, defauls to the Template name or TemplateName_Key for multi template files, parameters are: name or templateName and key or templateKey |
| DotJEMTemplateParameterPattern   | @\{(.+?)}               | A Regex pattern for finding parameters in templates | 
| DotJEMIncludeAllAdditionalFiles  | true                    | By default all files registered with AdditionalFiles are process unless they are explicitly excluded, if this is not desired this can be set to false |


Further each AdditionalFiles node may have these options set:

| Option              | Default                           | Description                                                            |
| ------------------- | --------------------------------- | ---------------------------------------------------------------------- |
| TemplateClass       | <file extention> + Templates      | The class name to put the templates into, defaults to the file extention in pascal case appended with Templates |
| Visibility          | <DotJEMTemplateVisibility>        | Overrides Global Option: DotJEMTemplateVisibility |
| Namespace           | <DotJEMTemplateNamespace>         | Overrides Global Option: DotJEMTemplateNamespace |
| MethodNameTemplate  | <DotJEMTemplateParameterPattern>  | Overrides Global Option: DotJEMTemplateParameterPattern |
| ParameterPattern    | <DotJEMTemplateParameterPattern>  | Overrides Global Option: DotJEMTemplateParameterPattern | 
| IsTemplates         | <DotJEMIncludeAllAdditionalFiles> | Overrides Global Option: DotJEMIncludeAllAdditionalFiles |
