using Lexing;

namespace OLangAst.EnumVariants;

public class EnumVariant(string name) : IAstNode
{
    public string Name = name;
    public SourceSpan Span { get; set; }
}