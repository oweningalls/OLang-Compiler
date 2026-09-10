using System.Reflection.Emit;
using Lexing;

namespace OLangAst.Miscellaneous;

public class CustomClass(TypeBuilder definedType) : IVariableType
{
    public TypeBuilder DefinedType = definedType;
    public SourceSpan Span { get; set; }
}