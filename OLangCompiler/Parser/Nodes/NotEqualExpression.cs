namespace OLangCompiler.Parser.Nodes;

public class NotEqualExpression(ITermNode lhs, IExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);