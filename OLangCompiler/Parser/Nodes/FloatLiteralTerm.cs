namespace OLangCompiler.Parser.Nodes;

public class FloatLiteralTerm : BaseTermNode
{
    public FloatLiteralTerm(float value)
    {
        Value = value;
        ValueType = TypeChecking.Types.ExpressionType.Float;
    }
    
    public readonly float Value;
}