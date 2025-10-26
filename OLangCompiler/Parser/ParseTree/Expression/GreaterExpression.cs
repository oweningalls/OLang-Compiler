namespace OLangCompiler.Parser.ParseTree.Expression;

public class GreaterExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);