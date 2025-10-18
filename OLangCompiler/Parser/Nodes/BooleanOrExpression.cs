namespace OLangCompiler.Parser.Nodes;

public class BooleanOrExpression(IExpressionNode lhs, IExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);