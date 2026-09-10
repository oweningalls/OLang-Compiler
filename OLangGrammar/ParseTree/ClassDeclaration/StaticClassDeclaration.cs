using Lexing;
using OLangGrammar.ParseTree.StmtList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.ClassDeclaration;

public class StaticClassDeclaration(IdentifierToken identifierToken, IClassMemberListNode classMemberList) : IClassDeclaration
{
    public IdentifierToken IdentifierToken = identifierToken;
    public IClassMemberListNode ClassMemberList = classMemberList;
    public SourceSpan Span { get; set; }
}