namespace OLangCompiler.Parser.ParseTree.Term;

public class BoolLiteralTerm : BaseTermNode
{
    public BoolLiteralTerm(bool value)
    {
        Value = value;
        ValueType = TypeChecking.Types.ExpressionType.Bool;
    }
    
    public readonly bool Value;
}