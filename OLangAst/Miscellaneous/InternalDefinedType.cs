using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Miscellaneous;

public class InternalDefinedType(IType definedType) : IVariableType
{
    public SourceSpan Span { get; set; }
    public string Name => throw new Exception("probably shouldn't check names for InternalDefinedType?");
    public IType DefinedType = definedType;
}