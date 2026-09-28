using Lexing;

namespace OLangAst.Miscellaneous;

public class CustomType(string name, bool isArray) : IVariableType
{
    public string Name { get; } = name;
    public bool IsArray { get; } = isArray;
    public SourceSpan Span { get; set; }
}