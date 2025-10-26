namespace OLangCompiler.Parser.ParseTree.Expression;

public class SubtractExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);