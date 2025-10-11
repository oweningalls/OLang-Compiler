namespace OLangCompiler.Parser.Nodes;

public class GreaterExpression(IExpressionNode lhs, IExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);