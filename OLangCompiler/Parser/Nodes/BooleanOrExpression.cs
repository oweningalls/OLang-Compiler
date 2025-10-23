namespace OLangCompiler.Parser.Nodes;

public class BooleanOrExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);