namespace OLangCompiler.Parser.ParseTree.Expression;

public class BooleanOrExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);