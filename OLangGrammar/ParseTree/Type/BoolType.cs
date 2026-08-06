using Lexing;

namespace OLangGrammar.ParseTree.Type;

public class BoolType : IType
{
    public SourceSpan Span { get; set; }
}