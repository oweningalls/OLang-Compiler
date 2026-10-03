using Lexing;

namespace OLangAst.TypeSystem;

public class EnumDeclaration(string identifier) : ITypeDeclaration
{
    public string Identifier = identifier;
    public DefinedType? Type;
    public SourceSpan Span { get; set; }
}