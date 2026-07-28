using OLangTokens.Tokens;

namespace OLangCompiler.Parser.ParseTree.Term;

public class IntLiteral(IntLiteralToken value) : ITerm
{
    public IntLiteralToken Value = value;
}