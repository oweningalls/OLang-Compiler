namespace OLangCompiler.Parser.Nodes;

public class TimesExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);