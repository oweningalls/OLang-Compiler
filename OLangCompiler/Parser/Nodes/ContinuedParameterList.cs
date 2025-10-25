using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public class ContinuedParameterList(ExpressionType type, IdentifierToken identifier, IParameterListNode parameterList) : Parameter(type, identifier)
{
    public IParameterListNode ParameterList = parameterList;
}