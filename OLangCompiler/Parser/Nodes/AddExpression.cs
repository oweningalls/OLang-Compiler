namespace OLangCompiler.Parser.Nodes;

public class AddExpression(IExpressionNode lhs, IExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);