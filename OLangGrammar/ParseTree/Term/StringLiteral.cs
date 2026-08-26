using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class StringLiteral(StringLiteralToken value) : ITerm
{
    public StringLiteralToken Value = value;
    public SourceSpan Span { get; set; }
}