using OLangCompiler.Parser.ParseTree.Type;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.ParameterList;

public class ContinuedParameterList(IType type, IdentifierToken identifier, IParameterListNode parameterList) : Parameter(type, identifier)
{
    public IParameterListNode ParameterList = parameterList;
}