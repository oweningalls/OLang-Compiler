using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class AreEqual(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs)
{
    public override IType? Type { get; set; } = PrimitiveTypes.BoolType;
}
