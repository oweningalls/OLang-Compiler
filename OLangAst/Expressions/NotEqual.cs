using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class NotEqual(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs)
{
    public override IType? Type { get; set; } = PrimitiveTypes.BoolType;
}
