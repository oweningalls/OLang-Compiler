using OLangCompiler.Parser.BottomUpParser;
using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Tokens;

namespace OLangTests;

class TestGrammar : IGrammar
{
    private List<BaseGrammarRule> _rules =
    [
        GrammarRule.Create((IBNode _, IntLiteralToken _) => new ANode()),
        GrammarRule.Create((IANode _, IntLiteralToken _) => new ANode()),
        GrammarRule.Create((IntLiteralToken _, IntLiteralToken _) => new ANode()),
        GrammarRule.Create((BoolLiteralToken _) => new BNode()),
    ];

    public Type GetStartSymbol()
    {
        return typeof(IANode);
    }

    public List<BaseGrammarRule> GetRules()
    {
        return _rules;
    }
}
class AmbiguousGrammar : IGrammar
{
    private List<BaseGrammarRule> _rules =
    [
        GrammarRule.Create((IBNode _) => new ANode()),
        GrammarRule.Create((IANode _) => new BNode()),
        GrammarRule.Create((IBNode _) => new BNode()),
        GrammarRule.Create((BoolLiteralToken _) => new BNode())
    ];

    public Type GetStartSymbol()
    {
        return typeof(IANode);
    }

    public List<BaseGrammarRule> GetRules()
    {
        return _rules;
    }
}

class Lr0Grammar : IGrammar
{
    private List<BaseGrammarRule> _rules =
    [
        GrammarRule.Create((BoolLiteralToken b, IntLiteralToken i) => new IntANode(b, i)),
        GrammarRule.Create((BoolLiteralToken b, FloatLiteralToken f) => new FloatANode(b, f)),
    ];
    public Type GetStartSymbol()
    {
        return typeof(IANode);
    }

    public List<BaseGrammarRule> GetRules()
    {
        return _rules;
    }
}

class ANode : IANode;

class BNode : IBNode;

interface IANode : INode;

interface IBNode : INode;

class FloatANode(BoolLiteralToken boolLiteralToken, FloatLiteralToken floatLiteralToken) : IANode
{
    public BoolLiteralToken BoolLiteralToken = boolLiteralToken;
    public FloatLiteralToken FloatLiteralToken = floatLiteralToken;
}

class IntANode(BoolLiteralToken boolLiteralToken, IntLiteralToken intLiteralToken) : IANode
{
    public BoolLiteralToken BoolLiteralToken = boolLiteralToken;
    public IntLiteralToken IntLiteralToken = intLiteralToken;
}