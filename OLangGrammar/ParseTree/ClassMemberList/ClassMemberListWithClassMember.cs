using Lexing;
using OLangGrammar.ParseTree.ClassMember;
using OLangGrammar.ParseTree.StmtList;

namespace OLangGrammar.ParseTree.ClassMemberList;

public class ClassMemberListWithClassMember(IClassMember classMember, IClassMemberListNode classMemberList) : IClassMemberListNode
{
    public IClassMember ClassMember = classMember;
    public IClassMemberListNode ClassMemberList = classMemberList;
    public SourceSpan Span { get; set; }
}