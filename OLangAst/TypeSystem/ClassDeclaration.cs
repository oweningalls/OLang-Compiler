using Lexing;
using OLangAst.ClassMembers;

namespace OLangAst.TypeSystem;

public class ClassDeclaration(string identifier, List<IClassMember> classMembers) : ITypeDeclaration
{
    public string Identifier = identifier;
    public List<IClassMember> ClassMembers = classMembers;
    public ConcreteType? Type; 
    public SourceSpan Span { get; set; }
}