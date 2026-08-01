using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class FloatLiteral(FloatLiteralToken value) : ITerm
{
    public FloatLiteralToken Value = value;
}