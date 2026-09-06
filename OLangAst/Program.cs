using Lexing;
using OLangAst.ClassMembers;
using OLangAst.Statements;

namespace OLangAst;

public class Program(List<IClassMember> classMembers) : IAstNode
{
    public List<IClassMember> ClassMembers = classMembers;
    public SourceSpan Span { get; set; }
}