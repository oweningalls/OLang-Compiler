namespace OLangCompiler.Parser.ParseTree.Expression;

public class NotEqualExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);