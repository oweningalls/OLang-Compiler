namespace OLangCompiler.Parser.ParseTree.Expression;

public class DoubleEqualsExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);