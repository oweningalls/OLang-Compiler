using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class BoolLiteral(BoolLiteralToken value) : ITerm
{
    public BoolLiteralToken Value = value;
}