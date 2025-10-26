namespace OLangCompiler.Parser.ParseTree.Term;

public class FloatLiteral : BaseTermNode
{
    public FloatLiteral(float value)
    {
        Value = value;
        ValueType = TypeChecking.Types.ExpressionType.Float;
    }
    
    public readonly float Value;
}