using OLangCompiler.Parser.ParseTree.ArgumentList;
using OLangCompiler.Parser.ParseTree.Term;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree;

public class InvocationNode(IdentifierToken identifier, IArgumentList argumentList) : ITerm
{
    public IdentifierToken Identifier = identifier;
    public IArgumentList Arguments = argumentList;
}