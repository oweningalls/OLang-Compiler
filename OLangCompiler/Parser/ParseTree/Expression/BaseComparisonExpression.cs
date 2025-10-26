namespace OLangCompiler.Parser.ParseTree.Expression;

public abstract class BaseComparisonExpression(BaseExpression lhs, BaseExpression rhs) : BaseBinaryExpression(lhs, rhs);