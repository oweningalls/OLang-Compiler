namespace OLangCompiler.Parser.Nodes;

public class SubtractExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);