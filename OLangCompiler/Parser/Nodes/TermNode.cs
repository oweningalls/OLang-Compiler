using OLangCompiler.Parser.Nodes.NodeValues;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class TermNode(OneOf.OneOf<IntLiteralToken, IdentifierToken> value) : INode
{
    public OneOf.OneOf<IntLiteralToken, IdentifierToken> Value = value;
}