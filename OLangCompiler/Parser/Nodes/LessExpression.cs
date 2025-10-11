namespace OLangCompiler.Parser.Nodes;

public class LessExpression(IExpressionNode lhs, IExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);