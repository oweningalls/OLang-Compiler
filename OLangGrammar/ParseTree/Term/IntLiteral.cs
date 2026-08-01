using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class IntLiteral(IntLiteralToken value) : ITerm
{
    public IntLiteralToken Value = value;
}