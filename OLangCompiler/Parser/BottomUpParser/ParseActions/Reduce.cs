namespace OLangCompiler.Parser.BottomUpParser.ParseActions;

public class Reduce(BaseGrammarRule rule) : IParseAction
{
    public readonly BaseGrammarRule Rule = rule;
}