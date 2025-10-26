namespace OLangCompiler.Parser.ParseTree.Expression;

public class AddExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);