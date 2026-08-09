using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.FunctionInvocation;

public class FunctionInvocation(IdentifierToken identifier) : IFunctionInvocation
{
    public IdentifierToken Identifier = identifier;
    public SourceSpan Span { get; set; }
}