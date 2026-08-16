using Lexing;

namespace OLangAst.Miscellaneous;

public class Parameter(string identifier, IVariableType type) : IAstNode
{
    public IVariableType Type = type;
    public string Identifier = identifier;
    public SourceSpan Span { get; set; }
}