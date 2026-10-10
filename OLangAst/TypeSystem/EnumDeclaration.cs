using Lexing;
using OLangAst.EnumVariants;

namespace OLangAst.TypeSystem;

public class EnumDeclaration(string identifier, List<string> typeParameters, List<EnumVariant> enumVariants) : ITypeDeclaration
{
    public string Identifier = identifier;
    public List<string> TypeParameters = typeParameters;
    public List<EnumVariant> EnumVariants = enumVariants;
    public ConcreteType? Type;
    public SourceSpan Span { get; set; }
}