using OLangGrammar.ParseTree.ArgumentList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.FunctionInvocation;

public class FunctionInvocation(IdentifierToken identifier, IArgumentList argumentList) : IFunctionInvocation
{
    public IdentifierToken Identifier = identifier;
    public IArgumentList Arguments = argumentList;
}