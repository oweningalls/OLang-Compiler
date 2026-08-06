using Lexing;

namespace OLangGrammar.ParseTree.Type;

public class FloatType : IType
{
    public SourceSpan Span { get; set; }
}