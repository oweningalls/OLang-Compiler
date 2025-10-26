namespace OLangCompiler.Parser.ParseTree.Expression;

public class BooleanAndExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);
