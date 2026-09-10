using Lexing;
using OLangAst.ClassMembers;

namespace OLangAst;

public class ClassDeclaration(bool isStatic, string identifier, List<IClassMember> classMembers) : IAstNode
{
    public bool IsStatic = isStatic;
    public string Identifier = identifier;
    public List<IClassMember> ClassMembers = classMembers;
    public SourceSpan Span { get; set; }
}