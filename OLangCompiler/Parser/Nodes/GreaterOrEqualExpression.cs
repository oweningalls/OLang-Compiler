namespace OLangCompiler.Parser.Nodes;

public class GreaterOrEqualExpression(IExpressionNode lhs, IExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);