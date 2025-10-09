namespace OLangCompiler.Parser.Nodes;

public class AddExpression(ITermNode lhs, IExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);