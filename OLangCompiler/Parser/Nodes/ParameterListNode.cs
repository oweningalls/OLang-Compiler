using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public class ParameterListNode(List<(ExpressionType, IdentifierToken)> parameters) : INode
{
    public List<(ExpressionType, IdentifierToken)> Parameters = parameters;
}