using Lexing;
using OLangGrammar.ParseTree.ArgumentList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.FunctionInvocation;

public class FunctionInvocationWithArguments(IdentifierToken identifier, IArgumentList argumentList) : IFunctionInvocation
{
    public IdentifierToken Identifier = identifier;
    public IArgumentList Arguments = argumentList;
    public SourceSpan Span { get; set; }
}