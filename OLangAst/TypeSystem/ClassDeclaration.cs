using Lexing;
using OLangAst.ClassMembers;

namespace OLangAst.TypeSystem;

public class ClassDeclaration(string identifier, List<IClassMember> classMembers) : ITypeDeclaration
{
    public string Identifier = identifier;
    public List<IClassMember> ClassMembers = classMembers;
    public DefinedType? Type; 
    public SourceSpan Span { get; set; }
}