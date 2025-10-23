namespace OLangCompiler.Parser.Nodes;

public class GreaterExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);