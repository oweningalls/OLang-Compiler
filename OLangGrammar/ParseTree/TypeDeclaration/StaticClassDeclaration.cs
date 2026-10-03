using Lexing;
using OLangGrammar.ParseTree.ClassMemberList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.TypeDeclaration;

public class StaticClassDeclaration(IdentifierToken identifierToken, IClassMemberListNode classMemberList) : ITypeDeclaration
{
    public IdentifierToken IdentifierToken = identifierToken;
    public IClassMemberListNode ClassMemberList = classMemberList;
    public SourceSpan Span { get; set; }
}