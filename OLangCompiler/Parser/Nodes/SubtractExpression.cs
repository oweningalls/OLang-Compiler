namespace OLangCompiler.Parser.Nodes;

public class SubtractExpression(ITermNode lhs, IExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);