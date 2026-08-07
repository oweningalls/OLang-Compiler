using Lexing;

namespace OLangAst;

public interface IAstNode
{
    public SourceSpan Span { get; set; }
}
