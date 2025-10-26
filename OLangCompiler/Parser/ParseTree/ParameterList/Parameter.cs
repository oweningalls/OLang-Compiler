using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.ParseTree.ParameterList;

public class Parameter(ExpressionType type, IdentifierToken identifier) : IParameterListNode
{
    public ExpressionType Type = type;
    public IdentifierToken Identifier = identifier;
}