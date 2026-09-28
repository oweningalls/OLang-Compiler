using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Miscellaneous;

public class InternalDefinedType(DefinedType type, bool isArray) : IVariableType
{
    public SourceSpan Span { get; set; }
    public string Name { get; } = type.Name;
    public bool IsArray { get; } = isArray;
    public DefinedType Type = type;
}