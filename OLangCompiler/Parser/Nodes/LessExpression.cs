namespace OLangCompiler.Parser.Nodes;

public class LessExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);