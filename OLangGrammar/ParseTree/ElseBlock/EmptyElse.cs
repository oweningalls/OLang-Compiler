using Lexing;

namespace OLangGrammar.ParseTree.ElseBlock;

public class EmptyElse : IElse
{
    public SourceSpan Span { get; set; }
}