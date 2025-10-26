using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.Term;

public class FloatLiteral(FloatLiteralToken value) : ITerm
{
    public FloatLiteralToken Value = value;
}