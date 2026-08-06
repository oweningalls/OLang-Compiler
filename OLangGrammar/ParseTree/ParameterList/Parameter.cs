using Lexing;
using OLangGrammar.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.ParameterList;

public class Parameter(IType type, IdentifierToken identifier) : IParameterListNode
{
    public IType Type = type;
    public IdentifierToken Identifier = identifier;
    public SourceSpan Span { get; set; }
}