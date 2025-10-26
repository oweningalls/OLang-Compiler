namespace OLangCompiler.Parser.ParseTree.Expression;

public class TimesExpression(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);