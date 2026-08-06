using Lexing;

namespace OLangGrammar.ParseTree.Type;

public class IntType : IType
{
    public SourceSpan Span { get; set; }
}