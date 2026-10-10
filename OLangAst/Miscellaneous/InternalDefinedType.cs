using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Miscellaneous;

public class InternalDefinedType(ConcreteType type) : IVariableType
{
    public SourceSpan Span { get; set; }
    public string Name { get; } = type.Name;
    public ConcreteType Type = type;
}