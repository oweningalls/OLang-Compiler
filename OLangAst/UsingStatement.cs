using Lexing;

namespace OLangAst;

public class UsingStatement(string module) : IAstNode
{
    public string Module = module;
    public SourceSpan Span { get; set; }
}