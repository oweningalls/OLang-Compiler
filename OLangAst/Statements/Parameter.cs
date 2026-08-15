using Lexing;
using OLangAst.Miscellaneous;

namespace OLangAst.Statements;

public class Parameter(string identifier, IVariableType type) : IAstNode
{
    public IVariableType Type = type;
    public string Identifier = identifier;
    public SourceSpan Span { get; set; }
}