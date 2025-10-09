namespace OLangCompiler.Parser.Nodes;

public class NotEqualExpression(IExpressionNode lhs, IExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);