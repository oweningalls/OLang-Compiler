namespace OLangCompiler.Parser.Nodes;

public class NotEqualExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);