using System.Reflection;
using System.Reflection.Emit;
using Lexing;

namespace OLangAst.Miscellaneous;

public class CustomClass(TypeBuilder definedType, bool isStatic) : IVariableType
{
    public TypeBuilder DefinedType = definedType;
    public bool IsStatic = isStatic;
    public SourceSpan Span { get; set; }


    public List<MethodInfo> Methods = [];
}