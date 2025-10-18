namespace OLangCompiler.Parser.Nodes;

public class BooleanAndExpression(IExpressionNode lhs, IExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);
