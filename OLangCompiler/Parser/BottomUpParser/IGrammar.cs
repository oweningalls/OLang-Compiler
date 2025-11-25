namespace OLangCompiler.Parser.BottomUpParser;

public interface IGrammar
{
    // THIS WILL BREAK THINGS IF IT'S NOT AN INTERFACE
    public Type GetStartSymbol();
    public List<BaseGrammarRule> GetRules();
}