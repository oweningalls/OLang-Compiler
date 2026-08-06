using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class BoolLiteral(BoolLiteralToken value) : ITerm
{
    public BoolLiteralToken Value = value;
    public SourceSpan Span { get; set; }
}