using OLangCompiler.Parser.ParseTree.ArgumentList;
using OLangTokens.Tokens;

namespace OLangCompiler.Parser.ParseTree.FunctionInvocation;

public class FunctionInvocation(IdentifierToken identifier, IArgumentList argumentList) : IFunctionInvocation
{
    public IdentifierToken Identifier = identifier;
    public IArgumentList Arguments = argumentList;
}