using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Miscellaneous;

public class InternalDefinedType(DefinedType type) : IVariableType
{
    public SourceSpan Span { get; set; }
    public string Name { get; } = type.Name;
    public DefinedType Type = type;
}