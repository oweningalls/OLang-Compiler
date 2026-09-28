using Lexing;
using OLangGrammar.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.ClassMember;

public class ArrayFieldDeclaration(IType arrayType, IdentifierToken identifier) : IClassMember
{
    public IType ArrayType = arrayType;
    public IdentifierToken Identifier = identifier;
    public SourceSpan Span { get; set; }
}