using OLangCompiler.Parser.ParseTree.Type;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.ParameterList;

public class Parameter(IType type, IdentifierToken identifier) : IParameterListNode
{
    public IType Type = type;
    public IdentifierToken Identifier = identifier;
}