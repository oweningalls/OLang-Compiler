namespace OLangAst.Expressions;

public class And(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs);