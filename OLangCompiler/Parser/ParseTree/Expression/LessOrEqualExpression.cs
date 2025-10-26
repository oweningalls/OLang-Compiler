namespace OLangCompiler.Parser.ParseTree.Expression;

public class LessOrEqualExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);