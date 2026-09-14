using Lexing;
using OLangAst.Miscellaneous;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class Cast(IVariableType type, IExpression value) : IExpression
{
    public IVariableType TargetType = type;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; }
}