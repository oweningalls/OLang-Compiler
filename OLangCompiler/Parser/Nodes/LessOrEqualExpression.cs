namespace OLangCompiler.Parser.Nodes;

public class LessOrEqualExpression(IExpressionNode lhs, IExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);