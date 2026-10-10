using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class Divide(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs)
{
    public override ConcreteType? Type { get; set; }
}
