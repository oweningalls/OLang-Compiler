using Lexing;
using OLangAst.ClassMembers;

namespace OLangAst;

public class ClassDeclaration(string identifier, List<IClassMember> classMembers) : IAstNode
{
    public string Identifier = identifier;
    public List<IClassMember> ClassMembers = classMembers;
    public SourceSpan Span { get; set; }
}