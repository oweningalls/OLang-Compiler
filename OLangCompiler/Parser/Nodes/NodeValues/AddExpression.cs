namespace OLangCompiler.Parser.Nodes.NodeValues;

public class AddExpression(TermNode lhs, ExpressionNode rhs)
{
    public TermNode Lhs = lhs;
    public ExpressionNode Rhs = rhs;
}