using Lexing;
using OLangAst.Miscellaneous;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class ArrayInstantiation(IVariableType arrayType, int size) : IExpression
{
    public IVariableType ArrayType = arrayType;
    public int Size = size;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; }
}