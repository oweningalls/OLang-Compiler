using Lexing;
using OLangAst.ClassMembers;
using OLangAst.TypeSystem;

namespace OLangAst;

public class ClassDeclaration(bool isStatic, string identifier, List<IClassMember> classMembers) : IAstNode
{
    public bool IsStatic = isStatic;
    public string Identifier = identifier;
    public List<IClassMember> ClassMembers = classMembers;
    public DefinedType? Type; 
    public SourceSpan Span { get; set; }
}