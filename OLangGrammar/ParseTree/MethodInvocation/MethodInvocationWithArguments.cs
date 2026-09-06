using Lexing;
using OLangGrammar.ParseTree.ArgumentList;
using OLangGrammar.ParseTree.Term;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.MethodInvocation;

public class MethodInvocationWithArguments(ITerm term, IdentifierToken identifier, IArgumentList argumentList) : IMethodInvocation
{
    public ITerm Term = term;
    public IdentifierToken Identifier = identifier;
    public IArgumentList Arguments = argumentList;
    public SourceSpan Span { get; set; }
}