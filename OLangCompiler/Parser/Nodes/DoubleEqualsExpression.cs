namespace OLangCompiler.Parser.Nodes;

public class DoubleEqualsExpression(IExpressionNode lhs, IExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);