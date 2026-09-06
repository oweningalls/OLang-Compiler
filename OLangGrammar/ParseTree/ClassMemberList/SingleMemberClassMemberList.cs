using Lexing;
using OLangGrammar.ParseTree.ClassMember;
using OLangGrammar.ParseTree.StmtList;

namespace OLangGrammar.ParseTree.ClassMemberList;

public class SingleMemberClassMemberList(IClassMember classMember) : IClassMemberListNode
{
    public IClassMember ClassMember = classMember;
    public SourceSpan Span { get; set; }
}