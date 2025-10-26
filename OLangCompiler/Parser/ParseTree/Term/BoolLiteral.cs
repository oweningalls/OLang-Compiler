namespace OLangCompiler.Parser.ParseTree.Term;

public class BoolLiteral : BaseTermNode
{
    public BoolLiteral(bool value)
    {
        Value = value;
        ValueType = TypeChecking.Types.ExpressionType.Bool;
    }
    
    public readonly bool Value;
}