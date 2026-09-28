using Lexing;
using OLangAst.Miscellaneous;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class ArrayInstantiation(IVariableType arrayType, IExpression size) : IExpression
{
    public IVariableType ArrayType = arrayType;
    public IExpression Size = size;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; }
}