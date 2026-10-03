using Lexing;

namespace OLangAst.Miscellaneous;

public class CustomType(string name) : IVariableType
{
    public string Name { get; } = name;
    public SourceSpan Span { get; set; }
}