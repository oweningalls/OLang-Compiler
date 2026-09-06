using Lexing;
using OLangGrammar.ParseTree.StmtList;

namespace OLangGrammar.ParseTree.Prog;

public class ProgramNode(IClassMemberListNode classMemberList) : IProgramNode
{
    public IClassMemberListNode ClassMemberList = classMemberList;
    public SourceSpan Span { get; set; }
}