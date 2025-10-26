namespace OLangCompiler.Parser.ParseTree.Expression;

public class BooleanOr(BaseExpression lhs, BaseExpression rhs) : BaseComparisonExpression(lhs, rhs);