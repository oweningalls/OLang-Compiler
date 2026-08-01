using OLangGrammar.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.ParameterList;

public class ContinuedParameterList(IType type, IdentifierToken identifier, IParameterListNode parameterList) : Parameter(type, identifier)
{
    public IParameterListNode ParameterList = parameterList;
}