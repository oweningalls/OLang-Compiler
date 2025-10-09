namespace OLangCompiler.Parser.Nodes;

public class SubtractExpression(IExpressionNode lhs, IExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);