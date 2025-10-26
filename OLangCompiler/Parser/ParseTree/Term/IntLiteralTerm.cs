namespace OLangCompiler.Parser.ParseTree.Term;

public class IntLiteralTerm : BaseTermNode
{
    public IntLiteralTerm(int value)
    {
        Value = value;
        ValueType = TypeChecking.Types.ExpressionType.Int;
    }
    
    public readonly int Value;
}