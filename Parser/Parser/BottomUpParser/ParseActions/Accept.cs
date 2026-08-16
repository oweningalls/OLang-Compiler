using Lexing;

namespace Parser.Parser.BottomUpParser.ParseActions;

public class Accept(INode node) : IParseAction
{
    public INode Node = node;
}