using OLangTokens.Tokens;

namespace OLangCompiler.Parser.ParseTree.Term;

public class BoolLiteral(BoolLiteralToken value) : ITerm
{
    public BoolLiteralToken Value = value;
}