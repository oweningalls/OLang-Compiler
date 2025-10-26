namespace OLangCompiler.Parser.BottomUpParser;

public interface IGrammar
{
    public List<IGrammarRule> GetRules();
}