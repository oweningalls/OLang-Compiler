namespace OLangCompiler.Parser.ParseTree.Expression;

public class LessExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseComparisonExpressionNode(lhs, rhs);