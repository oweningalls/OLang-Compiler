using Lexing;

namespace OLangGrammar.ParseTree.Type;

public class StringType : IType
{
    public SourceSpan Span { get; set; }
}