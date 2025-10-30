namespace OLangCompiler.Parser.BottomUpParser;

public interface IGrammar
{
    public Type GetStartSymbol();
    public List<BaseGrammarRule> GetRules();
}