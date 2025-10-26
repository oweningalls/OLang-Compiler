using OLangCompiler.Parser.ParseTree.ArgumentList;
using OLangCompiler.Parser.ParseTree.Term;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree;

public class InvocationNode(IdentifierToken identifier, IArgumentListNode argumentList) : BaseTermNode
{
    public IdentifierToken Identifier = identifier;
    public IArgumentListNode Arguments = argumentList;
}