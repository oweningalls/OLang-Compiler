using Lexing;

namespace OLangCompiler.Parser.BottomUpParser.Lr0;

public class Lr0Configuration
{
    private readonly BaseGrammarRule _rule;
    private readonly int _bookmark;
    public Lr0Configuration(BaseGrammarRule rule)
    {
        _rule = rule;
        _bookmark = 0;
    }

    public Lr0Configuration(Lr0Configuration configuration)
    {
        _rule = configuration._rule;
        _bookmark = configuration._bookmark + 1;
        if (_bookmark > _rule.GetRhsTypes().Count)
        {
            throw new Exception("Bookmark cannot be greater than the number of items in the RHS of the production");
        }
    }

    public Type? GetElementAfterBookmark()
    {
        return _bookmark == _rule.GetRhsTypes().Count ? null : _rule.GetRhsTypes()[_bookmark];
    }

    public bool IsShift()
    {
        return GetElementAfterBookmark() != null;
    }

    public bool IsReduce()
    {
        return GetElementAfterBookmark() == null;
    }

    public BaseGrammarRule GetRule()
    {
        return _rule;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Lr0Configuration other) return false;
        
        return other._bookmark == _bookmark && other._rule.Equals(_rule);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_bookmark, _rule);
    }

    public override string ToString()
    {
        var typesWithBookmark = _rule.GetRhsTypes().Select(x => x.Name).ToList();
        typesWithBookmark.Insert(_bookmark, ".");
        return $"{_rule.GetLhsType().Name} -> {string.Join(" ", typesWithBookmark)}";
    }
}