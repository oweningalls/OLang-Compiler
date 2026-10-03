using Lexing;
using OLangAst.ClassMembers;

namespace OLangAst.TypeSystem;

public class ClassDeclaration(bool isStatic, string identifier, List<IClassMember> classMembers) : ITypeDeclaration
{
    public bool IsStatic = isStatic;
    public string Identifier = identifier;
    public List<IClassMember> ClassMembers = classMembers;
    public DefinedType? Type; 
    public SourceSpan Span { get; set; }
}