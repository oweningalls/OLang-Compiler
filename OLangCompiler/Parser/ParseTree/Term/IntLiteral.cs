namespace OLangCompiler.Parser.ParseTree.Term;

public class IntLiteral : BaseTermNode
{
    public IntLiteral(int value)
    {
        Value = value;
        ValueType = TypeChecking.Types.ExpressionType.Int;
    }
    
    public readonly int Value;
}