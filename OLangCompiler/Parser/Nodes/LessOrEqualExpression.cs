namespace OLangCompiler.Parser.Nodes;

public class LessOrEqualExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);