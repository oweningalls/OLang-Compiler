using Lexing;
using Parser.Parser.BottomUpParser.ParseActions;

namespace Parser.Parser.BottomUpParser;

public interface ILrParseTable
{
    public IParseAction GetActionAndState(IGrammarElement? next, int state);
}