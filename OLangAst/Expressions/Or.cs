using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class Or(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs)
{
    public override ConcreteType? Type { get; set; } = PrimitiveTypes.BoolType;
}
