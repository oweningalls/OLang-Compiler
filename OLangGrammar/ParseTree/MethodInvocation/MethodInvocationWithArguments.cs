using Lexing;
using OLangGrammar.ParseTree.ArgumentList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.MethodInvocation;

public class MethodInvocationWithArguments(IdentifierToken type, IdentifierToken identifier, IArgumentList argumentList) : IMethodInvocation
{
    public IdentifierToken Type = type;
    public IdentifierToken Identifier = identifier;
    public IArgumentList Arguments = argumentList;
    public SourceSpan Span { get; set; }
}