namespace OLangCompiler.Parser.Nodes;

public class TimesExpression(IExpressionNode lhs, IExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);